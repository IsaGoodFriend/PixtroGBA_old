#include "load_data.h"
#include <string.h>


#include "sprites.h"
#include "core.h"
#include "physics.h"
#include "math.h"


#define FIXED2BLOCK(n)	((n) >> (ACC + BLOCK_SHIFT))
#define BLOCK2FIXED(n)	((n) << (ACC + BLOCK_SHIFT))

#define INT2BLOCK(n)	((n) >> (BLOCK_SHIFT))
#define BLOCK2INT(n)	((n) << (BLOCK_SHIFT))

#define FIXED2TILE(n)	((n) >> (ACC + 3))
#define TILE2FIXED(n)	((n) << (ACC + 3))

#define INT2TILE(n)		((n) >> 3)
#define TILE2INT(n)		((n) << 3)

#define VIS_BLOCK_POS(x, y) (((x) & 0x1F) + (((y) & 0x1F) << 5))

#define X_TILE_BUFFER 1
#define Y_TILE_BUFFER 1

#define BLOCK_X		30
#define BLOCK_Y		20

#define BGOFS			((vu16*)(REG_BASE+0x0010))

unsigned short *uncmp_visuals = (unsigned short*)0x02002000;
unsigned char *lvl_info;

extern int width, height, y_shift;
extern unsigned char *collision_data;

extern int cam_x, cam_y, prev_cam_x, prev_cam_y;

extern char level_meta[128];

void load_collision(unsigned char *level_start){
	
	lvl_info = level_start;
	
	width =  lvl_info[0];
	height = lvl_info[1];
	lvl_info += 2;
	
	int index = width - 1;
	y_shift = 0;
	
	while (index) {
		index >>= 1;
		++y_shift;
	}
	
	while (lvl_info[0] < 128) {
		level_meta[lvl_info[0]] = lvl_info[1];	
		lvl_info += 2;
	}
	lvl_info++;
	
	unsigned int count = (lvl_info[0]);
	unsigned int value = (lvl_info[1]);
	
	lvl_info += 2;
	
	unsigned char* cpyColl = collision_data;
	
	int indexX, indexY;
	
	// decompress file data
	// --- COLLISION ---
	int r1 = (1 << y_shift) - width;
	unsigned int countT;
	
	for (indexY = 0; indexY < height; ++indexY){// by row
		countT = (count > width) ? width : count; // get the mem set count value.  if larger than the width
		
		for (indexX = 0; indexX < width;)
		{
			memset(cpyColl, value, countT);
			count -= countT;
			indexX += countT;
			cpyColl += countT;
			
			if (indexX < width)
			{
				count = lvl_info[0];
				
				countT = (count > (width - indexX)) ? (width - indexX) : count;	
				
				value = lvl_info[1];
				lvl_info += 2;
			}
		}
		cpyColl += r1;
	}
}
void load_midground(int index) {
	
	int r1 = (1 << y_shift) - width, indexX, indexY;
	unsigned int count = lvl_info[0], countT;
	unsigned int value = (lvl_info[2] << 8) | lvl_info[1];
	
	lvl_info += 3;
	
	unsigned short* cpyColl = &uncmp_visuals[0x2000 * index];
	
	for (indexY = 0; indexY < height; ++indexY){// by row
		countT = (count > width) ? width : count; // get the mem set count value.  if larger than the width
		
		for (indexX = 0; indexX < width;) {
			int a = countT;
			while (--a >= 0)
				cpyColl[a] = value;
			
			count -= countT;
			indexX += countT;
			cpyColl += countT;
			
			if (indexX < width)
			{
				count = lvl_info[0];
				
				countT = (count > (width - indexX)) ? (width - indexX) : count;	
				
				value = lvl_info[2] << 8;
				value |= lvl_info[1];
				
				lvl_info += 3;
			}
		}
		cpyColl += r1;
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
		
		if (cam_x < BLOCK_SIZE)
			cam_x = BLOCK_SIZE;
		if (cam_y < BLOCK_SIZE)
			cam_y = BLOCK_SIZE;
			
		if (cam_x + 240 + BLOCK_SIZE > BLOCK2INT(width))
			cam_x = BLOCK2INT(width) - 240 - BLOCK_SIZE;
		if (cam_y + 160 + BLOCK_SIZE > BLOCK2INT(height))
			cam_y = BLOCK2INT(height) - 160 - BLOCK_SIZE;
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
	int i, vis, pos;
	
	if (0) {
		
		vis = data[0];
		
		len--;
		screen++;
		data++;
	}
	
	for (i = 0; i < (len & 0xFE); i += 2) {
		
		vis = data[i >> 1];
		screen[i] =		vis | ((y ^ (vis >> 10)) & 0x1) | ((( vis >> 11) & 0x1) << 1);
		screen[i + 1] =	vis | ((y ^ (vis >> 10)) & 0x1) | (((~vis >> 11) & 0x1) << 1);
	}
	
	if (len & 0x1) {
		
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
				
				int vis = uncmp_visuals[(startX >> 1) + ((min >> 1) << y_shift)];
				foreground[position] = vis | ((min ^ (vis >> 10)) & 0x1) | (((startX ^ (vis >> 11)) & 0x1) << 1);
				if (midground) {
					vis = uncmp_visuals[(startX >> 1) + ((min >> 1) << y_shift) + 0x2000];
					midground[position] = vis | ((min ^ (vis >> 10)) & 0x1) | (((startX ^ (vis >> 11)) & 0x1) << 1);
				}
				if (background) {
					vis = uncmp_visuals[(startX >> 1) + ((min >> 1) << y_shift) + 0x4000];
					background[position] = vis | ((min ^ (vis >> 10)) & 0x1) | (((startX ^ (vis >> 11)) & 0x1) << 1);
				}
			}
			startX += dirX;
		}
		
		if (startY != endY) {
			
			min = startX - (BLOCK_X + 2) * dirX;
			max = startX;
			
			for (; min != max; min += dirX){
				position = VIS_BLOCK_POS(min, startY);
				
				int vis = uncmp_visuals[(min >> 1) + ((startY >> 1) << y_shift)];
				foreground[position] = vis | ((startY ^ (vis >> 10)) & 0x1) | (((min ^ (vis >> 11)) & 0x1) << 1);
				if (midground) {
					vis = uncmp_visuals[(startX >> 1) + ((min >> 1) << y_shift) + 0x2000];
					midground[position] = vis | ((startY ^ (vis >> 10)) & 0x1) | (((min ^ (vis >> 11)) & 0x1) << 1);
				}
				if (background) {
					vis = uncmp_visuals[(startX >> 1) + ((min >> 1) << y_shift) + 0x4000];
					background[position] = vis | ((startY ^ (vis >> 10)) & 0x1) | (((min ^ (vis >> 11)) & 0x1) << 1);;
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
//*
void reset_cam() {
	
	cam_x -= 120;
	cam_y -= 80;
	
	protect_cam();
	
	if (foreground_count == 0)
		goto skip_loadcam;
	
	int val;
	
	int x = INT2TILE(cam_x);
	int y = INT2TILE(cam_y);
	
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
		int p2 = ((y >> 1) << y_shift) + (x >> 1);
		
		copy_tiles(&foreground[p1], &uncmp_visuals[p2], x, y, 32 - x);
		
		/*
		if (midground)
			memcpy(&midground[p1], &uncmp_visuals[p2 + 0x2000], 64 - (x << 1));
		if (background)
			memcpy(&background[p1], &uncmp_visuals[p2 + 0x4000], 64 - (x << 1));
		
		*/
		p1 &= 0xFE0;
		p2 += (32 - x) >> 1;
		
		copy_tiles(&foreground[p1], &uncmp_visuals[p2], 0, y, x);
		/*
		memcpy(&foreground[p1], &uncmp_visuals[p2], x << 1);
		if (midground)
			memcpy(&midground[p1], &uncmp_visuals[p2 + 0x2000], (x << 1));
		if (background)
			memcpy(&background[p1], &uncmp_visuals[p2 + 0x4000], (x << 1));
		
		*/
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
				
				foreground[position] = uncmp_visuals[startX + (min << y_shift)];
				if (midground)
					midground[position]  = uncmp_visuals[startX + (min << y_shift) + 0x2000];
				if (background)
					background[position] = uncmp_visuals[startX + (min << y_shift) + 0x4000];
			}
			startX += dirX;
		}
		
		if (startY != endY) {
			
			min = startX - (BLOCK_X + 2) * dirX;
			max = startX;
			
			for (; min != max; min += dirX){
				position = VIS_BLOCK_POS(min, startY);
				
				foreground[position] = uncmp_visuals[min + (startY << y_shift)];
				if (midground)
					midground[position] = uncmp_visuals[min + (startY << y_shift) + 0x2000];
				if (background)
					background[position] = uncmp_visuals[min + (startY << y_shift) + 0x4000];
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
		int p2 = (y << y_shift) + x;
		++y;
		
		memcpy(&foreground[p1], &uncmp_visuals[p2], 64 - (x << 1));
		if (midground)
			memcpy(&midground[p1], &uncmp_visuals[p2 + 0x2000], 64 - (x << 1));
		if (background)
			memcpy(&background[p1], &uncmp_visuals[p2 + 0x4000], 64 - (x << 1));
		
		p1 &= 0xFE0;
		p2 += 32 - x;
		
		memcpy(&foreground[p1], &uncmp_visuals[p2], x << 1);
		if (midground)
			memcpy(&midground[p1], &uncmp_visuals[p2 + 0x2000], (x << 1));
		if (background)
			memcpy(&background[p1], &uncmp_visuals[p2 + 0x4000], (x << 1));
		
	}
	
	skip_loadcam:
	
	prev_cam_x = cam_x;
	prev_cam_y = cam_y;
	
	cam_x += 120;
	cam_y += 80;
}
//*/
#endif