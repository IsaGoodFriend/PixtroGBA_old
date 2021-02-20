#include "load_data.h"
#include <string.h>


#include "sprites.h"
#include "core.h"
#include "physics.h"
#include "math.h"


#define FIXED2BLOCK(n) ((n) >> (ACC + BLOCK_SHIFT))
#define BLOCK2FIXED(n) ((n) << (ACC + BLOCK_SHIFT))

#define INT2BLOCK(n) ((n) >> (BLOCK_SHIFT))
#define BLOCK2INT(n) ((n) << (BLOCK_SHIFT))

#define VIS_BLOCK_POS(x, y) (((x) & 0x1F) + (((y) & 0x1F) << 5))

#define X_TILE_BUFFER 1
#define Y_TILE_BUFFER 1

#ifdef LARGE_TILES

#define BLOCK_X		15
#define BLOCK_Y		10

#else

#define BLOCK_X		30
#define BLOCK_Y		20

#endif

#define BGOFS			((vu16*)(REG_BASE+0x0010))

unsigned short *uncmp_visuals = (unsigned short*)0x02002000;
unsigned char *lvlInfo;

extern int width, height, yShift;
extern unsigned char *collisionData;

extern int foreground_count;

extern int cam_x, cam_y, prev_cam_x, prev_cam_y;

extern char level_meta[128];

void load_collision(unsigned char *levelInfo){
	
	lvlInfo = levelInfo;
	
	width =  lvlInfo[0];
	height = lvlInfo[1];
	lvlInfo += 2;
	
	int index = width - 1;
	yShift = 0;
	
	while (index) {
		index >>= 1;
		++yShift;
	}
	
	while (lvlInfo[0] < 128) {
		level_meta[lvlInfo[0]] = lvlInfo[1];	
		lvlInfo += 2;
	}
	lvlInfo++;
	
	unsigned int count = (lvlInfo[0]);
	unsigned int value = (lvlInfo[1]);
	
	lvlInfo += 2;
	
	unsigned char* cpyColl = collisionData;
	
	int indexX, indexY;
	
	// decompress file data
	// --- COLLISION ---
	int r1 = (1 << yShift) - width;
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
				count = lvlInfo[0];
				
				countT = (count > (width - indexX)) ? (width - indexX) : count;	
				
				value = lvlInfo[1];
				lvlInfo += 2;
			}
		}
		cpyColl += r1;
	}
}
void load_midground(int index) {
	
	int r1 = (1 << yShift) - width, indexX, indexY;
	unsigned int count = lvlInfo[0], countT;
	unsigned int value = (lvlInfo[2] << 8) | lvlInfo[1];
	
	lvlInfo += 3;
	
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
				count = lvlInfo[0];
				
				countT = (count > (width - indexX)) ? (width - indexX) : count;	
				
				value = lvlInfo[2] << 8;
				value |= lvlInfo[1];
				
				lvlInfo += 3;
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
			
			if (ENT_FLAG(PERSISTENT, entities[index].flags))
			{
				entities[max_entities] = entities[index];
				
				++max_entities;
			}
			else{
				entities[index].flags = 0;
			}
			
		}
	}
	
	// load entities
	unsigned char type;
	type = lvlInfo[0];
	lvlInfo += 1;
	
	while (type != 255) {
		unsigned char
			x = lvlInfo[0],
			y = lvlInfo[1];
		
		lvlInfo += 2;
		
		entities[max_entities].velX = 0;
		entities[max_entities].velY = 0;
		
		entities[max_entities].x = BLOCK2FIXED(x);
		entities[max_entities].y = BLOCK2FIXED(y);
		entities[max_entities].ID = type | ENT_VISIBLE_FLAG;
		
		if (entity_inits[type])
			lvlInfo += entity_inits[type](&max_entities, lvlInfo);
		
		++max_entities;
		
		type = lvlInfo[0];
		
		lvlInfo += 1;
	} 
}

#ifdef LARGE_TILES

#else
void move_cam() {
	if (foreground_count == 0)
		return;
	
	int moveX = INT2BLOCK(cam_x) - INT2BLOCK(prev_cam_x),
		moveY = INT2BLOCK(cam_y) - INT2BLOCK(prev_cam_y);
	
	if (!moveX && !moveY)
		return;
	
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
				
				foreground[position] = uncmp_visuals[startX + (min << yShift)];
				if (midground)
					midground[position]  = uncmp_visuals[startX + (min << yShift) + 0x2000];
				if (background)
					background[position] = uncmp_visuals[startX + (min << yShift) + 0x4000];
			}
			startX += dirX;
		}
		
		if (startY != endY) {
			
			min = startX - (BLOCK_X + 2) * dirX;
			max = startX;
			
			for (; min != max; min += dirX){
				position = VIS_BLOCK_POS(min, startY);
				
				foreground[position] = uncmp_visuals[min + (startY << yShift)];
				if (midground)
					midground[position] = uncmp_visuals[min + (startY << yShift) + 0x2000];
				if (background)
					background[position] = uncmp_visuals[min + (startY << yShift) + 0x4000];
			}
			startY += dirY;
		}
	}
	while (startX != endX || startY != endY);
	
}
//*
void reset_cam() {
	if (foreground_count == 0)
		return;
	
	cam_x = 0x8;
	cam_y = 0x8;
	
	int val;
	for (val = 0; val < foreground_count << 1; val += 2) {
		BGOFS[val] 		= cam_x;
		BGOFS[val + 1]	= cam_y;
	}
	
	
	int x = INT2BLOCK(cam_x);
	int y = INT2BLOCK(cam_y);
	
	y -= Y_TILE_BUFFER;
	x -= X_TILE_BUFFER;
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
		int p2 = (y << yShift) + x;
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
	
}
//*/
#endif