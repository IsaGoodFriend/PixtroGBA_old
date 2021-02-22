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

unsigned int tile_types[128];

unsigned int y_shift;
unsigned int lvl_width, lvl_height;
unsigned char *collision_data = (unsigned char*)0x02008000;

int GetBlock(int x, int y) {
	x -= x * (x < 0);
	y -= y * (y < 0);
	x += ((lvl_width  - 1) - x) * (x >= lvl_width);
	y += ((lvl_height - 1) - y) * (y >= lvl_height);
	
	return collision_data[x + (y << y_shift)];
}


// Physics Collision for any entity
// Returns y collision data on 0x00007FFF, and x collision data on 0x7FFF0000.
// 0x80000000 is set if x velocity was positive, and 0x00008000 is set if y velocity was positive
unsigned int entity_physics(Entity *ent, int hit_mask, int detect_mask) {
	
	
	// Get the sign (-/+) of the velocity components
	int sign_x = (ent->vel_x >> 31) | 1, sign_y = (ent->vel_y >> 31) | 1;
	int y_is_pos= -(~(ent->vel_y) >> 31); // If y is positive, equals 1, else 0;
	int y_is_neg = ent->vel_y >> 31; // If y is negative, equals -1, else 0;
	int x_is_pos= -(~(ent->vel_x) >> 31); // If x is positive, equals 1, else 0;
	int x_is_neg = ent->vel_x >> 31; // If x is negative, equals -1, else 0;
	
	// Box collision indexes - Tile values;
	int index = 0, index2 = 0;
	
	int top = ent->y,
		bot = ent->y + INT2FIXED(ent->height),
		lef = ent->x					 - x_is_neg * (ent->vel_x),
		rgt = ent->x + INT2FIXED(ent->width)  + x_is_pos * (ent->vel_x);
	
	//Get the start and end of the base collisionbox
	int y_min = ent->y - y_is_neg * (INT2FIXED(ent->height) - 1),
		y_max = ent->y + y_is_pos * (INT2FIXED(ent->height) - 1),
		x_min = ent->x - x_is_neg * (INT2FIXED(ent->width)  - 1),
		x_max = ent->x + x_is_pos * (INT2FIXED(ent->width)  - 1);
	
	// Block values that were hit - flag
	int hit_value_x = 0, hit_value_y = 0;
	
	int offsetX = 0xFFFFFF, offsetY = 0xFFFFFF;
	
	for (index = FIXED2BLOCK(x_min); index != FIXED2BLOCK(x_max + ent->vel_x) + sign_x; index += sign_x){
		for (index2 = FIXED2BLOCK(y_min); index2 != FIXED2BLOCK(y_max) + sign_y; index2 += sign_y){
			
			int shape = GetBlock(index, index2);
			if (!shape)
				continue;
			
			shape = tile_types[shape - 1];
			int type = (shape & TILE_TYPE_MASK) >> TILE_TYPE_SHIFT;
			int mask = 1 << (type - 1);
			
			if (!type || !(mask & detect_mask)) // If the block is 0 or if the block is not solid, ignore
				continue;
			
			shape = shape & TILE_SHAPE_MASK;
			
			switch (shape) {
			case SHAPE_FULL:
				break;
			default:
				continue;
			}
			
			int temp_offset = (BLOCK2FIXED(index - x_is_neg) - INT2FIXED(ent->width * x_is_pos)) - ent->x; // Get offsets to align to grid
			
			if (INT_ABS(temp_offset) < INT_ABS(offsetX)) { // If new movement is smaller, set collision data.
				offsetX = temp_offset; // Set offset
				hit_value_x = mask;
			}
			else if (temp_offset == offsetX)
			{
				hit_value_x |= mask;
			}
			
			if (hit_value_x & hit_mask)
				break;
		}
		if (hit_value_x & detect_mask){
			ent->x += offsetX;
			offsetX += ent->vel_x;
			
			if (ent->vel_x != 0 && sign_x == INT_SIGN((BLOCK2FIXED(index) + 0x400) - (ent->x + (ent->width >> 1))))
				ent->vel_x = 0;
			else
				hit_value_x = 0;
			break;
		}
	}
	ent->x += ent->vel_x * !(hit_value_x & detect_mask);
	
	x_min = ent->x - x_is_neg * (INT2FIXED(ent->width) - 1);
	x_max = ent->x + x_is_pos * (INT2FIXED(ent->width) - 1);
	
	for (index = FIXED2BLOCK(y_min); index != FIXED2BLOCK(y_max + ent->vel_y) + sign_y; index += sign_y){
		for (index2 = FIXED2BLOCK(x_min); index2 != FIXED2BLOCK(x_max) + sign_x; index2 += sign_x){
			
			int shape = GetBlock(index2, index);
			if (!shape)
				continue;
			
			shape = tile_types[shape - 1];
			int type = (shape & TILE_TYPE_MASK) >> TILE_TYPE_SHIFT;
			int mask = 1 << (type - 1);
			
			if (!type || !(mask & detect_mask)) // If the block is 0 or if the block is not solid, ignore
				continue;
			
			shape = shape & TILE_SHAPE_MASK;
			
			switch (shape) {
			case SHAPE_FULL:
				break;
			case SHAPE_PLATFORM:
				if (ent->vel_y < 0)
					continue;
				break;
			default:
				continue;
			}
			
			int temp_offset = (BLOCK2FIXED(index - y_is_neg) - INT2FIXED(ent->height * y_is_pos)) - ent->y;
			
			if (INT_ABS(temp_offset) < INT_ABS(offsetY)) { // If new movement is smaller, set collision data.
				offsetY = temp_offset; // Set offset
				hit_value_y |= mask;
			}
			else if (temp_offset == offsetY)
			{
				hit_value_y |= mask;
			}
			
			if (hit_value_x & hit_mask)
				break;
		}
		if (hit_value_y & detect_mask){
			ent->y += offsetY;
			offsetY += ent->vel_y;
			
			if (ent->vel_y != 0 && sign_y == INT_SIGN((BLOCK2FIXED(index) + 0x400) - (ent->y + (ent->height >> 1))))
				ent->vel_y = 0;
			else
				hit_value_y = 0;
			break;
		}
	}
	ent->y += ent->vel_y * !(hit_value_y & detect_mask);
	
	hit_value_x &= detect_mask;
	hit_value_y &= detect_mask;
	
	return (hit_value_x << 16) | hit_value_y;
}
/*
unsigned int collide_rect(int x, int y, int width, int height){
	int y_min = y,
	y_max = y + INT2FIXED(height) - 1;
	int x_min = x,
	x_max = x + INT2FIXED(width)  - 1;	
	
	// Block values that were hit - flag
	int blockValue;
	int hitValue = 0;
	int xCoor, yCoor;
	
	for (xCoor = FIXED2BLOCK(x_min); xCoor != FIXED2BLOCK(x_max) + 1; ++xCoor){
		for (yCoor = FIXED2BLOCK(y_min); yCoor != FIXED2BLOCK(y_max) + 1; ++yCoor){
			
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