#include "load_data.h"
#include <string.h>

#include "core.h"
#include "physics.h"
#include "math.h"
#include "loading.h"

#define FIXED2BLOCK(n) ((n) >> (ACC + BLOCK_SHIFT))
#define BLOCK2FIXED(n) ((n) << (ACC + BLOCK_SHIFT))

#define INT2BLOCK(n) ((n) >> (BLOCK_SHIFT))
#define BLOCK2INT(n) ((n) << (BLOCK_SHIFT))

#define FIXED2TILE(n) ((n) >> (ACC + 3))
#define TILE2FIXED(n) ((n) << (ACC + 3))

#define INT2TILE(n) ((n) >> 3)
#define TILE2INT(n) ((n) << 3)

#define VIS_BLOCK_POS(x, y) (((x)&0x1F) + (((y)&0x1F) << 5))

#define X_TILE_BUFFER (1 * BLOCK_SIZE)
#define Y_TILE_BUFFER (1 * BLOCK_SIZE)

#define BLOCK_X 30
#define BLOCK_Y 20

#define BGOFS ((vu16 *)(REG_BASE + 0x0010))

#define TILE_INFO ((unsigned short *)0x02010000)

#define LEVEL_REGION_A (unsigned short *)0x02020000
#define LEVEL_REGION_B (unsigned short *)0x02030000

// the char array in rom of the current level being loaded
unsigned char *lvl_info;
// the short array where the level is currently loaded to in ram
unsigned short *level_toload;
// the start of the current level
extern unsigned short *tileset_data;
unsigned int unloaded_entities[64];
int unload_index;

unsigned short test_values[256];

int level_loading;

extern int lvl_width, lvl_height, loading_width, loading_height;

extern int cam_x, cam_y, prev_cam_x, prev_cam_y;

extern char level_meta[128];

void set_loading_region(char region_b)
{
	level_toload = region_b ? LEVEL_REGION_B : LEVEL_REGION_A;
}
void set_entities_location()
{
	unsigned int *level_ptr = (unsigned int *)level_toload;
	*level_ptr = lvl_info;
	level_ptr += 1;

	level_toload = level_ptr;
}

void load_midground()
{
	lvl_info += lvl_info[0];

	int size = lvl_info[0] | (lvl_info[1] << 8);

	lvl_info += 2;

	LZ77UnCompWram(lvl_info, level_toload);

	lvl_info += size;

	level_toload += (loading_width * loading_height) << 0;
}
void load_entities()
{
	tileset_data += 2;

	int index;
	for (index = 0; index < 128; ++index)
	{
		level_meta[index] = 0;
	}

	index = *tileset_data;

	while (index != 0xFFFF)
	{
		tileset_data++;
		level_meta[(index & 0xFF)] = index >> 8;
		index = *tileset_data;
	}
	tileset_data++;

	lvl_info = tileset_data;
	lvl_info += lvl_width * lvl_height * 2;

	max_entities = 0;

	// unload entities
	for (; index < ENTITY_LIMIT; ++index)
	{

		if (ENT_FLAG(PERSISTENT, index))
		{
			entities[max_entities] = entities[index];

			++max_entities;
		}
		else
		{
			entities[index].flags[0] = 0;
			entities[index].flags[1] = 0;
			entities[index].flags[2] = 0;
			entities[index].flags[3] = 0;
		}
	}

	while (lvl_info[3] != 0x08)
		lvl_info--;

	lvl_info = *((unsigned int **)lvl_info);

	// load entities
	int type;
	type = lvl_info[0];
	lvl_info += 1;

	while (type != 255)
	{
		int x = lvl_info[0],
			y = lvl_info[1];

		lvl_info += 2;

		entities[max_entities].vel_x = 0;
		entities[max_entities].vel_y = 0;

		entities[max_entities].x = BLOCK2FIXED(x);
		entities[max_entities].y = BLOCK2FIXED(y);
		entities[max_entities].ID = type | (level_loading << 8);
		entities[max_entities].flags[4] = ENT_VISIBLE_FLAG | ENT_ACTIVE_FLAG;

		char is_loading = 1;

		int i;
		for (i = 0; i < 64; ++i)
		{
		}

		if (entity_inits[type])
			lvl_info += entity_inits[type](max_entities, lvl_info, &is_loading);

		if (is_loading)
			++max_entities;

		type = lvl_info[0];
		lvl_info += 1;
	}
}

void protect_cam()
{
	if (foreground_count)
	{ // TODO: add a flag that can disable this feature

		if (cam_x < X_TILE_BUFFER)
			cam_x = X_TILE_BUFFER;
		if (cam_y < Y_TILE_BUFFER)
			cam_y = Y_TILE_BUFFER;

		if (cam_x + 240 - X_TILE_BUFFER > BLOCK2INT(lvl_width))
			cam_x = BLOCK2INT(lvl_width) - 240 - X_TILE_BUFFER;
		if (cam_y + 160 - Y_TILE_BUFFER > BLOCK2INT(lvl_height))
			cam_y = BLOCK2INT(lvl_height) - 160 - Y_TILE_BUFFER;
	}

	int i;
	for (i = 0; i < foreground_count; ++i)
	{
		BGOFS[(i << 1) + 0] = cam_x;
		BGOFS[(i << 1) + 1] = cam_y;
	}
	for (; i < 4; ++i)
	{
		BGOFS[(i << 1) + 0] = FIXED_MULT(cam_x, 0x80);
		BGOFS[(i << 1) + 1] = FIXED_MULT(cam_y, 0x80);
	}
}
#ifdef LARGE_TILES
void copy_tiles(unsigned short *screen, unsigned short *data, int x, int y, int len)
{
	int i;

	if (x & 0x1)
	{
		int vis = data[0];
		if (vis != 0)
		{
			vis--;

			int tile = TILE_INFO[((vis & 0xFF) << 2) | ((y ^ (vis >> 11)) & 0x1) | (((~vis >> 10) & 0x1) << 1)];

			tile ^= vis & 0x0C00; // Flip Index
			tile += vis & 0xF000; // Palette
			screen[0] = tile;
		}

		len--;
		screen++;
		data++;
	}

	int testIndex = 0;

	for (i = (len & ~0x1) - 2; i >= 0; i -= 2)
	{
		int vis = data[i >> 1];
		if (vis == 0)
			continue;
		vis--;

		int tile = TILE_INFO[((vis & 0xFF) << 2) | ((y ^ (vis >> 11)) & 0x1) | (((vis >> 10) & 0x1) << 1)];

		tile ^= vis & 0x0C00; // Flip Index
		tile += vis & 0xF000; // Palette

		screen[i] = tile;

		tile = TILE_INFO[((vis & 0xFF) << 2) | ((y ^ (vis >> 11)) & 0x1) | (((~vis >> 10) & 0x1) << 1)];

		tile ^= vis & 0x0C00; // Flip Index
		tile += vis & 0xF000; // Palette
		screen[i + 1] = tile;
	}

	if (len & 0x1)
	{
		int vis = data[len >> 1];
		if (vis != 0)
		{
			vis--;

			int tile = TILE_INFO[((vis & 0xFF) << 2) | ((y ^ (vis >> 11)) & 0x1) | (((vis >> 10) & 0x1) << 1)];

			tile ^= vis & 0x0C00; // Flip Index
			tile += vis & 0xF000; // Palette
			screen[len - 1] = tile;
		}
	}
}
void move_cam()
{
	cam_x -= 120;
	cam_y -= 80;

	protect_cam();

	if (foreground_count == 0)
		goto skip_loadcam;

	int moveX = INT2TILE(cam_x) - INT2TILE(prev_cam_x),
		moveY = INT2TILE(cam_y) - INT2TILE(prev_cam_y);

	if (!moveX && !moveY)
		goto skip_loadcam;

	int xMin = INT2TILE(SIGNED_MIN(prev_cam_x, cam_x)) - 1;
	int xMax = INT2TILE(SIGNED_MAX(prev_cam_x, cam_x)) + BLOCK_X + 1;
	int yMin = INT2TILE(SIGNED_MIN(prev_cam_y, cam_y)) - 1;
	int yMax = INT2TILE(SIGNED_MAX(prev_cam_y, cam_y)) + BLOCK_Y + 1;

	unsigned short *foreground = se_mem[31];
	unsigned short *midground = se_mem[30];
	unsigned short *background = se_mem[29];

	if (foreground_count <= 2)
		background = NULL;
	if (foreground_count <= 1)
		midground = NULL;

	int position;

	// Get the start X and Y rows needed to edit, and the direction each is needed to move.
	// Get the end destinations for x and y.

	int dirX = INT_SIGN(moveX), dirY = INT_SIGN(moveY);
	int startX = (dirX < 0) ? xMin : xMax, startY = (dirY < 0) ? yMin : yMax;
	int endX = startX + moveX, endY = startY + moveY;
	int min, max;

	if (startX != endX)
		startX -= dirX;
	if (startY != endY)
		startY -= dirY;

	if (dirX == 0)
		dirX = 1;
	if (dirY == 0)
		dirY = 1;

	do
	{

		if (startX != endX)
		{
			min = startY - (BLOCK_Y + 2) * dirY;
			max = startY;

			for (; min != max; min += dirY)
			{
				position = VIS_BLOCK_POS(startX, min);

				int vis = tileset_data[(startX >> 1) + ((min >> 1) * lvl_width)];

				int tile = TILE_INFO[((vis & 0xFF) << 2) | ((min ^ (vis >> 11)) & 0x1) | (((startX ^ (vis >> 10)) & 0x1) << 1)];
				tile ^= vis & 0x0C00;
				tile += vis & 0xF000;
				foreground[position] = tile;

				if (midground)
				{
					tile = TILE_INFO[((vis & 0xFF) << 2) | ((min ^ (vis >> 11)) & 0x1) | (((startX ^ (vis >> 10)) & 0x1) << 1)];
					tile ^= vis & 0x0C00;
					tile += vis & 0xF000;
					midground[position] = tile;
				}
				if (background)
				{
					tile = TILE_INFO[((vis & 0xFF) << 2) | ((min ^ (vis >> 11)) & 0x1) | (((startX ^ (vis >> 10)) & 0x1) << 1)];
					tile ^= vis & 0x0C00;
					tile += vis & 0xF000;
					background[position] = tile;
				}
			}
			startX += dirX;
		}

		if (startY != endY)
		{

			min = startX - (BLOCK_X + 2) * dirX;
			max = startX;

			for (; min != max; min += dirX)
			{
				position = VIS_BLOCK_POS(min, startY);

				int vis = tileset_data[(min >> 1) + ((startY >> 1) * lvl_width)];

				int tile = TILE_INFO[((vis & 0xFF) << 2) | ((startY ^ (vis >> 11)) & 0x1) | (((min ^ (vis >> 10)) & 0x1) << 1)];
				tile ^= vis & 0x0C00;
				tile += vis & 0xF000;
				foreground[position] = tile;

				if (midground)
				{
					tile = TILE_INFO[((vis & 0xFF) << 2) | ((startY ^ (vis >> 11)) & 0x1) | (((min ^ (vis >> 10)) & 0x1) << 1)];
					tile ^= vis & 0x0C00;
					tile += vis & 0xF000;
					midground[position] = tile;
				}
				if (background)
				{
					tile = TILE_INFO[((vis & 0xFF) << 2) | ((startY ^ (vis >> 11)) & 0x1) | (((min ^ (vis >> 10)) & 0x1) << 1)];
					tile ^= vis & 0x0C00;
					tile += vis & 0xF000;
					background[position] = tile;
				}
			}
			startY += dirY;
		}
	} while (startX != endX || startY != endY);

skip_loadcam:

	prev_cam_x = cam_x;
	prev_cam_y = cam_y;

	cam_x += 120;
	cam_y += 80;
}
void reset_cam()
{
	// Get top left position
	cam_x -= 120;
	cam_y -= 80;

	protect_cam();

	// If there are no layers to set, don't change anything
	if (foreground_count == 0)
		goto skip_loadcam;

	int val;

	int x = INT2TILE(cam_x);
	int y = INT2TILE(cam_y);

	//x &= ~0x1;

	unsigned short *foreground = se_mem[31];
	unsigned short *midground = se_mem[30];
	unsigned short *background = se_mem[29];

	if (foreground_count < 3)
		background = 0;
	if (foreground_count < 2)
		midground = 0;

	val = 22;
	while (val-- > 0)
	{

		int p1 = VIS_BLOCK_POS(x, y);
		int p2 = ((y >> 1) * lvl_width) + (x >> 1);

		copy_tiles(&foreground[p1], &tileset_data[p2], x, y, 32 - x);

		p1 &= 0xFE0;
		p2 += (32 - x) >> 1;

		copy_tiles(&foreground[p1], &tileset_data[p2], 0, y, x);

		++y;
	}

skip_loadcam:

	prev_cam_x = cam_x;
	prev_cam_y = cam_y;

	cam_x += 120;
	cam_y += 80;
}
#else
void move_cam()
{

	cam_x -= 120;
	cam_y -= 80;

	protect_cam();

	if (foreground_count == 0)
		goto skip_loadcam;

	int moveX = INT2BLOCK(cam_x) - INT2BLOCK(prev_cam_x),
		moveY = INT2BLOCK(cam_y) - INT2BLOCK(prev_cam_y);

	if (!moveX && !moveY)
		goto skip_loadcam;

	int xMin = INT2BLOCK(SIGNED_MIN(prev_cam_x, cam_x)) - 1;
	int xMax = INT2BLOCK(SIGNED_MAX(prev_cam_x, cam_x)) + BLOCK_X + 1;
	int yMin = INT2BLOCK(SIGNED_MIN(prev_cam_y, cam_y)) - 1;
	int yMax = INT2BLOCK(SIGNED_MAX(prev_cam_y, cam_y)) + BLOCK_Y + 1;

	unsigned short *foreground = se_mem[31];
	unsigned short *midground = se_mem[30];
	unsigned short *background = se_mem[29];

	if (foreground_count <= 2)
		background = NULL;
	if (foreground_count <= 1)
		midground = NULL;

	int position;

	// Get the start X and Y rows needed to edit, and the direction each is needed to move.
	// Get the end destinations for x and y.

	int dirX = INT_SIGN(moveX), dirY = INT_SIGN(moveY);
	int startX = (dirX < 0) ? xMin : xMax, startY = (dirY < 0) ? yMin : yMax;
	int endX = startX + moveX, endY = startY + moveY;
	int min, max;

	if (startX != endX)
		startX -= dirX;
	if (startY != endY)
		startY -= dirY;

	if (dirX == 0)
		dirX = 1;
	if (dirY == 0)
		dirY = 1;

	do
	{

		if (startX != endX)
		{
			min = startY - (BLOCK_Y + 2) * dirY;
			max = startY;

			for (; min != max; min += dirY)
			{
				position = VIS_BLOCK_POS(startX, min);

				foreground[position] = tileset_data[startX + (min * lvl_width)];
				if (midground)
					midground[position] = tileset_data[startX + (min * lvl_width) + 0x2000];
				if (background)
					background[position] = tileset_data[startX + (min * lvl_width) + 0x4000];
			}
			startX += dirX;
		}

		if (startY != endY)
		{

			min = startX - (BLOCK_X + 2) * dirX;
			max = startX;

			for (; min != max; min += dirX)
			{
				position = VIS_BLOCK_POS(min, startY);

				foreground[position] = tileset_data[min + (startY * lvl_width)];
				if (midground)
					midground[position] = tileset_data[min + (startY * lvl_width) + 0x2000];
				if (background)
					background[position] = tileset_data[min + (startY * lvl_width) + 0x4000];
			}
			startY += dirY;
		}
	} while (startX != endX || startY != endY);

skip_loadcam:

	prev_cam_x = cam_x;
	prev_cam_y = cam_y;

	cam_x += 120;
	cam_y += 80;
}
void reset_cam()
{

	cam_x -= 120;
	cam_y -= 80;

	protect_cam();

	if (foreground_count == 0)
		goto skip_loadcam;

	int val;

	int x = INT2BLOCK(cam_x);
	int y = INT2BLOCK(cam_y);

	x &= ~0x1;

	unsigned short *foreground = se_mem[31];
	unsigned short *midground = se_mem[30];
	unsigned short *background = se_mem[29];

	if (foreground_count < 3)
		background = 0;
	if (foreground_count < 2)
		midground = 0;

	val = 22;
	while (val-- > 0)
	{

		int p1 = VIS_BLOCK_POS(x, y);
		int p2 = (y * lvl_width) + x;
		++y;

		memcpy(&foreground[p1], &tileset_data[p2], 64 - (x << 1));
		if (midground)
			memcpy(&midground[p1], &tileset_data[p2 + 0x2000], 64 - (x << 1));
		if (background)
			memcpy(&background[p1], &tileset_data[p2 + 0x4000], 64 - (x << 1));

		p1 &= 0xFE0;
		p2 += 32 - x;

		memcpy(&foreground[p1], &tileset_data[p2], x << 1);
		if (midground)
			memcpy(&midground[p1], &tileset_data[p2 + 0x2000], (x << 1));
		if (background)
			memcpy(&background[p1], &tileset_data[p2 + 0x4000], (x << 1));
	}

skip_loadcam:

	prev_cam_x = cam_x;
	prev_cam_y = cam_y;

	cam_x += 120;
	cam_y += 80;
}
#endif