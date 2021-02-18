//*
#include <tonc.h>

#include "physics.h"
#include "math.h"
#include "core.h"

#define FIXED2BLOCK(n) ((n) >> (ACC + BLOCK_SHIFT))
#define BLOCK2FIXED(n) ((n) << (ACC + BLOCK_SHIFT))

#define TILE_TYPE_SHIFT		4
#define TILE_TYPE_MASK 		0x000000F0
#define TILE_SHAPE_MASK		0x0000000F

#define SHAPE_FULL			0
#define SHAPE_PLATFORM		1

unsigned int tileTypes[32];

unsigned char yShift;
unsigned int width, height;
unsigned char collisionData[4096];

int GetBlock(int x, int y){
	x -= x * (x < 0);
	y -= y * (y < 0);
	x += ((width  - 1) - x) * (x >= width);
	y += ((height - 1) - y) * (y >= height);
	
	return collisionData[x + (y << yShift)];
}


// Physics Collision for any entity
// Returns y collision data on 0x00007FFF, and x collision data on 0x7FFF0000.
// 0x80000000 is set if x velocity was positive, and 0x00008000 is set if y velocity was positive
unsigned int entity_physics(Entity *ent, int hitMask, int detectMask) {
	
	
	// Get the sign (-/+) of the velocity components
	int signX = (ent->velX >> 31) | 1, signY = (ent->velY >> 31) | 1;
	int yIsPos= -(~(ent->velY) >> 31); // If y is positive, equals 1, else 0;
	int yIsNeg = ent->velY >> 31; // If y is negative, equals -1, else 0;
	int xIsPos= -(~(ent->velX) >> 31); // If x is positive, equals 1, else 0;
	int xIsNeg = ent->velX >> 31; // If x is negative, equals -1, else 0;
	
	// Box collision indexes - Tile values;
	int index = 0, index2 = 0;
	
	int top = ent->y,
		bot = ent->y + INT2FIXED(height),
		lef = ent->x					 - xIsNeg * (ent->velX),
		rgt = ent->x + INT2FIXED(width)  + xIsPos * (ent->velX);
	
	//Get the start and end of the base collisionbox
	int yMin = ent->y - yIsNeg * (INT2FIXED(height) - 1),
		yMax = ent->y + yIsPos * (INT2FIXED(height) - 1),
		xMin = ent->x - xIsNeg * (INT2FIXED(width)  - 1),
		xMax = ent->x + xIsPos * (INT2FIXED(width)  - 1);
	
	// Block values that were hit - flag
	int hitValueX = 0, hitValueY = 0;
	
	int offsetX = 0xFFFFFF, offsetY = 0xFFFFFF;
	
	for (index = FIXED2BLOCK(xMin); index != FIXED2BLOCK(xMax + ent->velX) + signX; index += signX){
		for (index2 = FIXED2BLOCK(yMin); index2 != FIXED2BLOCK(yMax) + signY; index2 += signY){
			
			int shape = GetBlock(index, index2);
			int type = (shape & TILE_TYPE_MASK) >> TILE_TYPE_SHIFT;
			int mask = 1 << (type - 1);
			
			if (!shape || !(tileTypes[shape - 1] & TILE_TYPE_MASK) || !(mask & detectMask)) // If the block is 0 or if the block is not solid, ignore
				continue;
			
			shape = shape & TILE_SHAPE_MASK;
			
			switch (shape) {
			case SHAPE_FULL:
				break;
			default:
				continue;
			}
			
			int tempOffset = (BLOCK2FIXED(index - xIsNeg) - INT2FIXED(width * xIsPos)) - ent->x; // Get offsets to align to grid
			
			if (INT_ABS(tempOffset) < INT_ABS(offsetX)) { // If new movement is smaller, set collision data.
				offsetX = tempOffset; // Set offset
				hitValueX = mask;
			}
			else if (tempOffset == offsetX)
			{
				hitValueX |= mask;
			}
			
			tempOffset = (BLOCK2FIXED(index + xIsPos) - INT2FIXED(width * -xIsNeg)) - ent->x; 
			if (INT_ABS(tempOffset) < INT_ABS(offsetX)) { // If new movement is smaller, set collision data.
				offsetX = tempOffset; // Set offset
				hitValueX = mask;
			}
			else if (tempOffset == offsetX)
			{
				hitValueX |= mask;
			}
			
			if (hitValueX & hitMask)
				break;
		}
		if (hitValueX & detectMask){
			ent->x += offsetX;
			offsetX += ent->velX;
			
			if (ent->velX != 0 && signX == INT_SIGN((BLOCK2FIXED(index) + 0x400) - (ent->x + (width >> 1))))
				ent->velX = 0;
			else
				hitValueX = 0;
			break;
		}
	}
	ent->x += ent->velX * !(hitValueX & detectMask);
	
	xMin = ent->x - xIsNeg * (INT2FIXED(width) - 1);
	xMax = ent->x + xIsPos * (INT2FIXED(width) - 1);
	
	for (index = FIXED2BLOCK(yMin); index != FIXED2BLOCK(yMax + ent->velY) + signY; index += signY){
		for (index2 = FIXED2BLOCK(xMin); index2 != FIXED2BLOCK(xMax) + signX; index2 += signX){
			
			int shape = GetBlock(index, index2);
			int type = (shape & TILE_TYPE_MASK) >> TILE_TYPE_SHIFT;
			int mask = 1 << (type - 1);
			
			if (!shape || !(tileTypes[shape - 1] & TILE_TYPE_MASK) || !(mask & detectMask)) // If the block is 0 or if the block is not solid, ignore
				continue;
			
			shape = shape & TILE_SHAPE_MASK;
			
			switch (shape) {
			case SHAPE_FULL:
				break;
			case SHAPE_PLATFORM:
				if (ent->velY < 0)
					continue;
				break;
			default:
				continue;
			}
			
			int tempOffset = (BLOCK2FIXED(index - yIsNeg) - INT2FIXED(height * yIsPos)) - ent->y;
			
			if (INT_ABS(tempOffset) < INT_ABS(offsetY)) { // If new movement is smaller, set collision data.
				offsetY = tempOffset; // Set offset
				hitValueY |= mask;
			}
			else if (tempOffset == offsetY)
			{
				hitValueY |= mask;
			}
			
			tempOffset = (BLOCK2FIXED(index + yIsPos) - INT2FIXED(height * -yIsNeg)) - ent->y;
			if (INT_ABS(tempOffset) < INT_ABS(offsetY)) { // If new movement is smaller, set collision data.
				offsetY = tempOffset; // Set offset
				hitValueY = mask;
			}
			else if (tempOffset == offsetY)
			{
				hitValueY |= mask;
			}
			
			if (hitValueX & hitMask)
				break;
		}
		if (hitValueY & detectMask){
			ent->y += offsetY;
			offsetY += ent->velY;
			
			if (ent->velY != 0 && signY == INT_SIGN((BLOCK2FIXED(index) + 0x400) - (ent->y + (height >> 1))))
				ent->velY = 0;
			else
				hitValueY = 0;
			break;
		}
	}
	ent->y += ent->velY * !(hitValueY & detectMask);
	
	return (hitValueX << 16) | hitValueY;
}
/*
unsigned int collide_rect(int x, int y, int width, int height){
	int yMin = y,
	yMax = y + INT2FIXED(height) - 1;
	int xMin = x,
	xMax = x + INT2FIXED(width)  - 1;	
	
	// Block values that were hit - flag
	int blockValue;
	int hitValue = 0;
	int xCoor, yCoor;
	
	for (xCoor = FIXED2BLOCK(xMin); xCoor != FIXED2BLOCK(xMax) + 1; ++xCoor){
		for (yCoor = FIXED2BLOCK(yMin); yCoor != FIXED2BLOCK(yMax) + 1; ++yCoor){
			switch (GetBlock(xCoor, yCoor)) {
			case BLOCK_SOLID:
				hitValue |= COLLISION_SOLID;
				break;
			case BLOCK_NOTE1:
				if (~GAME_music_beat & NOTE_BLOCK_BEAT)
					hitValue |= COLLISION_SOLID;
				break;
			case BLOCK_NOTE2:
				if (GAME_music_beat & NOTE_BLOCK_BEAT)
					hitValue |= COLLISION_SOLID;
				break;
			case BLOCK_DREAM:
				hitValue |= COLLISION_DREAM;
				break;
			case BLOCK_WATER:
				hitValue |= COLLISION_WATER;
				break;
			case BLOCK_STRAWB:
				hitValue |= COLLISION_STRAWB;
				break;
			case BLOCK_PLATFORM:
				if (IN_BACKGROUND)
					break;
				if (FIXED2BLOCK(yMax + 0x700) <= yCoor){
					hitValue |= COLLISION_PLATFORM;
				}
				break;
			case BLOCK_SPIKELEFT:
				if (entities[0].velX > 0 || FIXED2BLOCK(xMin + 0x500) > xCoor)
					break;
				hitValue |= COLLISION_HARM | COLLISION_WALLBLOCK;
				break;
			case BLOCK_SPIKERIGHT:
				if (entities[0].velX < 0 || FIXED2BLOCK(xMax - 0x500) < xCoor)
					break;
				hitValue |= COLLISION_HARM | COLLISION_WALLBLOCK;
				break;
			case BLOCK_SPINNER:
			case BLOCK_SPIKEUP:
			case BLOCK_SPIKEDOWN:
				hitValue |= COLLISION_HARM;
				break;
			}
		}
	}
	
	return hitValue;
}
unsigned int collide_entity(unsigned int ID){
	int i = 0;
	
	Entity *id = &entities[ID], *iterP;
	
	int id_LX = FIXED2INT(id->x);
	int id_LY = FIXED2INT(id->y);
	int id_RX = id_LX + id->width - 1;
	int id_RY = id_LY + id->height - 1;
	int iter_LX, iter_LY, iter_RX, iter_RY;
	
	for (; i < ACTOR_LIMIT; ++i)
	{
		if (i == ID)
			continue;
		
		iterP = &entities[i];
		
		if (!ACTOR_ENABLED(iterP->flags) || !ACTOR_CAN_COLLIDE(iterP->flags))
			continue;
		
		iter_LX = FIXED2INT(iterP->x);
		iter_LY = FIXED2INT(iterP->y);
		iter_RX = iter_LX + iterP->width - 1;
		iter_RY = iter_LY + iterP->height - 1;
		
		if (id_RX < iter_LX || iter_RX < id_LX || id_RY < iter_LY || iter_RY < id_LY)
			continue;
		
		return iterP->ID;
	}
	return 0xFFFFFFFF;
}
//*/