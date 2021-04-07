#include "load_data.h"
#include <string.h>

#include "sprites.h"
#include "core.h"
#include "physics.h"
#include "math.h"
#include "loading.h"


#define FIXED2BLOCK(n)	((n) >> (ACC + BLOCK_SHIFT))
#define BLOCK2FIXED(n)	((n) << (ACC + BLOCK_SHIFT))

#define INT2BLOCK(n)	((n) >> (BLOCK_SHIFT))
#define BLOCK2INT(n)	((n) << (BLOCK_SHIFT))

#define FIXED2TILE(n)	((n) >> (ACC + 3))
#define TILE2FIXED(n)	((n) << (ACC + 3))

#define INT2TILE(n)		((n) >> 3)
#define TILE2INT(n)		((n) << 3)

#define VIS_BLOCK_POS(x, y) (((x) & 0x1F) + (((y) & 0x1F) << 5))

#define X_TILE_BUFFER (1 * BLOCK_SIZE)
#define Y_TILE_BUFFER (1 * BLOCK_SIZE)

#define BLOCK_X		30
#define BLOCK_Y		20

#define BGOFS			((vu16*)(REG_BASE+0x0010))

#define TILE_INFO			((unsigned short*)0x02002000)

#define LEVEL_REGION_A		 (unsigned short*)0x02020000
#define LEVEL_REGION_B		 (unsigned short*)0x02030000

unsigned char *lvl_info;

extern int lvl_width, lvl_height;
extern unsigned short *tileset_data;

extern int cam_x, cam_y, prev_cam_x, prev_cam_y;

extern char level_meta[256];

void set_level_region(char region_b) {
	tileset_data = region_b ? LEVEL_REGION_B : LEVEL_REGION_A;
}

void load_collision(unsigned char *level_start){
	
	load_header(level_start);
	
	unsigned int count = (lvl_info[0]);
	unsigned int value = (lvl_info[1]);
	
	lvl_info += 2;
	
	unsigned char* cpyColl = tileset_data;
	
	int indexX, indexY;
	
	// decompress file data
	// --- COLLISION ---
	int r1 = (1 * lvl_width) - lvl_width;
	unsigned int countT;
	
	for (indexY = 0; indexY < lvl_height; ++indexY){// by row
		countT = (count > lvl_width) ? lvl_width : count; // get the mem set count value.  if larger than the width
		
		for (indexX = 0; indexX < lvl_width;)
		{
			memset(cpyColl, value, countT);
			count -= countT;
			indexX += countT;
			cpyColl += countT;
			
			if (indexX < lvl_width)
			{
				count = lvl_info[0];
				
				countT = (count > (lvl_width - indexX)) ? (lvl_width - indexX) : count;	
				
				value = lvl_info[1];
				lvl_info += 2;
			}
		}
		cpyColl += r1;
	}
}
void load_midgrounds(int index) {
	
	int indexX, indexY;
	unsigned int count = lvl_info[0], countT;
	unsigned int value = (lvl_info[2] << 8) | lvl_info[1];
	
	lvl_info += 3;
	
	unsigned short* cpyColl = &tileset_data[0x2000 * index];
	
	for (indexY = 0; indexY < lvl_height; ++indexY){// by row
		countT = (count > lvl_width) ? lvl_width : count; // get the mem set count value.  if larger than the width
		
		for (indexX = 0; indexX < lvl_width;) {
			int a = countT;
			while (--a >= 0)
				cpyColl[a] = value;
			
			count -= countT;
			indexX += countT;
			cpyColl += countT;
			
			if (indexX < lvl_width)
			{
				count = lvl_info[0];
				
				countT = (count > (lvl_width - indexX)) ? (lvl_width - indexX) : count;	
				
				value = lvl_info[2] << 8;
				value |= lvl_info[1];
				
				lvl_info += 3;
			}
		}
	}
}
void load_entities() {
	
	max_entities = 0;
	
	// unload entities
	{
		int index = 0;
		for (; index < ENTITY_LIMIT; ++index){
			
			if (ENT_FLAG(PERSISTENT, index))
			{
				entities[max_entities] = entities[index];
				
				++max_entities;
			}
			else{
				entities[index].flags[0] = 0;
				entities[index].flags[1] = 0;
			}
			
		}
	}
	
	// load entities
	unsigned char type;
	type = lvl_info[0];
	lvl_info += 1;
	
	while (type != 255) {
		unsigned char
			x = lvl_info[0],
			y = lvl_info[1];
		
		lvl_info += 2;
		
		entities[max_entities].vel_x = 0;
		entities[max_entities].vel_y = 0;
		
		entities[max_entities].x = BLOCK2FIXED(x);
		entities[max_entities].y = BLOCK2FIXED(y);
		entities[max_entities].ID = type | ENT_VISIBLE_FLAG;
		
		if (entity_inits[type])
			lvl_info += entity_inits[type](&max_entities, lvl_info);
		
		++max_entities;
		
		type = lvl_info[0];
		
		lvl_info += 1;
	} 
}

void protect_cam(){
	
	if (foreground_count) {// TODO: add a flag that can disable this feature
		
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
	for (i = 0; i < foreground_count; ++i) {
		BGOFS[(i << 1) + 0] = cam_x;
		BGOFS[(i << 1) + 1] = cam_y;
	}
	for (; i < 4; ++i) {
		BGOFS[(i << 1) + 0] = FIXED_MULT(cam_x, 0x80);
		BGOFS[(i << 1) + 1] = FIXED_MULT(cam_y, 0x80);
	}
}
#ifdef LARGE_TILES
void copy_tiles(unsigned short *screen, unsigned short *data, int x, int y, int len) {
	int i;

	if (x & 0x1) {
		int vis = data[0];

		int tile		= TILE_INFO[((vis & 0xFF) << 2)  | ((y ^ (vis >> 11)) & 0x1) | (((~vis >> 10) & 0x1) << 1)];
		tile ^= vis & 0x0C00;
		tile += vis & 0xF000;
		screen[0]	= tile;

		len--;
		screen++;
		data++;
	}
	
	for (i = (len & ~0x1) - 2; i >= 0; i -= 2) {
		
		int vis = data[i >> 1];
		
		int tile		= TILE_INFO[((vis & 0xFF) << 2) | ((y ^ (vis >> 11)) & 0x1) | ((( vis >> 10) & 0x1) << 1)];
		tile ^= vis & 0x0C00;
		tile += vis & 0xF000;
		screen[i]		= tile;
		
		tile			= TILE_INFO[((vis & 0xFF) << 2)  | ((y ^ (vis >> 11)) & 0x1) | (((~vis >> 10) & 0x1) << 1)];
		tile ^= vis & 0x0C00;
		tile += vis & 0xF000;
		screen[i + 1]	= tile;
	}
	
	if (len & 0x1) {
		int vis = data[len >> 1];
		
		int tile		= TILE_INFO[((vis & 0xFF) << 2) | ((y ^ (vis >> 11)) & 0x1) | ((( vis >> 10) & 0x1) << 1)];
		tile ^= vis & 0x0C00;
		tile += vis & 0xF000;
		screen[len - 1]	= tile;
	}
}
void move_cam() {

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
	unsigned short *midground  = se_mem[30];
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
	
	do {
		
		if (startX != endX) {
			min = startY - (BLOCK_Y + 2) * dirY;
			max = startY;
			
			for (; min != max; min += dirY){
				position = VIS_BLOCK_POS(startX, min);
				
				int vis = tileset_data[(startX >> 1) + ((min >> 1) * lvl_width)];
				
				int tile		= TILE_INFO[((vis & 0xFF) << 2) | ((min ^ (vis >> 11)) & 0x1) | (((startX ^ (vis >> 10)) & 0x1) << 1)];
				tile ^= vis & 0x0C00;
				tile += vis & 0xF000;
				foreground[position] = tile;

				if (midground) {
					tile		= TILE_INFO[((vis & 0xFF) << 2) | ((min ^ (vis >> 11)) & 0x1) | (((startX ^ (vis >> 10)) & 0x1) << 1)];
					tile ^= vis & 0x0C00;
					tile += vis & 0xF000;
					midground[position] = tile;
				}
				if (background) {
					tile		= TILE_INFO[((vis & 0xFF) << 2) | ((min ^ (vis >> 11)) & 0x1) | (((startX ^ (vis >> 10)) & 0x1) << 1)];
					tile ^= vis & 0x0C00;
					tile += vis & 0xF000;
					background[position] = tile;
				}
			}
			startX += dirX;
		}
		
		if (startY != endY) {
			
			min = startX - (BLOCK_X + 2) * dirX;
			max = startX;
			
			for (; min != max; min += dirX){
				position = VIS_BLOCK_POS(min, startY);
				
				int vis = tileset_data[(min >> 1) + ((startY >> 1) * lvl_width)];
				
				int tile		= TILE_INFO[((vis & 0xFF) << 2)| ((startY ^ (vis >> 11)) & 0x1) | (((min ^ (vis >> 10)) & 0x1) << 1)];
				tile ^= vis & 0x0C00;
				tile += vis & 0xF000;
				foreground[position] = tile;

				if (midground) {
					tile		= TILE_INFO[((vis & 0xFF) << 2)| ((startY ^ (vis >> 11)) & 0x1) | (((min ^ (vis >> 10)) & 0x1) << 1)];
					tile ^= vis & 0x0C00;
					tile += vis & 0xF000;
					midground[position] = tile;
				}
				if (background) {
					tile		= TILE_INFO[((vis & 0xFF) << 2)| ((startY ^ (vis >> 11)) & 0x1) | (((min ^ (vis >> 10)) & 0x1) << 1)];
					tile ^= vis & 0x0C00;
					tile += vis & 0xF000;
					background[position] = tile;
				}
			}
			startY += dirY;
		}
	}
	while (startX != endX || startY != endY);
	
	skip_loadcam:
	
	prev_cam_x = cam_x;
	prev_cam_y = cam_y;
	
	cam_x += 120;
	cam_y += 80;
}
void reset_cam() {
	
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
	unsigned short *midground  = se_mem[30];
	unsigned short *background = se_mem[29];
	
	if (foreground_count < 3)
		background = 0;
	if (foreground_count < 2)
		midground = 0;
	
	val = 32;
	while (val-- > 0){
		
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
void move_cam() {
	
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
	unsigned short *midground  = se_mem[30];
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
	
	do {
		
		if (startX != endX) {
			min = startY - (BLOCK_Y + 2) * dirY;
			max = startY;
			
			for (; min != max; min += dirY){
				position = VIS_BLOCK_POS(startX, min);
				
				foreground[position] = tileset_data[startX + (min * lvl_width)];
				if (midground)
					midground[position]  = tileset_data[startX + (min * lvl_width) + 0x2000];
				if (background)
					background[position] = tileset_data[startX + (min * lvl_width) + 0x4000];
			}
			startX += dirX;
		}
		
		if (startY != endY) {
			
			min = startX - (BLOCK_X + 2) * dirX;
			max = startX;
			
			for (; min != max; min += dirX){
				position = VIS_BLOCK_POS(min, startY);
				
				foreground[position] = tileset_data[min + (startY * lvl_width)];
				if (midground)
					midground[position] = tileset_data[min + (startY * lvl_width) + 0x2000];
				if (background)
					background[position] = tileset_data[min + (startY * lvl_width) + 0x4000];
			}
			startY += dirY;
		}
	}
	while (startX != endX || startY != endY);
	
	skip_loadcam:
	
	prev_cam_x = cam_x;
	prev_cam_y = cam_y;
	
	cam_x += 120;
	cam_y += 80;
}
void reset_cam() {
	
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
	unsigned short *midground  = se_mem[30];
	unsigned short *background = se_mem[29];
	
	if (foreground_count < 3)
		background = 0;
	if (foreground_count < 2)
		midground = 0;
	
	val = 32;
	while (val-- > 0){
		
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