//*
#include "tonc_vscode.h"

#include "physics.h"
#include "math.h"
#include "core.h"

#define FIXED2BLOCK(n) ((n) >> (ACC + BLOCK_SHIFT))
#define BLOCK2FIXED(n) ((n) << (ACC + BLOCK_SHIFT))

#define INT2BLOCK(n) ((n) >> (BLOCK_SHIFT))
#define BLOCK2INT(n) ((n) << (BLOCK_SHIFT))

#define TILE_TYPE_SHIFT		8
#define TILE_TYPE_MASK 		0x0000FF00
#define TILE_SHAPE_MASK		0x000000FF

unsigned int tile_types[256];

unsigned int lvl_width, lvl_height;
unsigned short *tileset_data;

int get_block(int x, int y) {
	x -= x * (x < 0);
	y -= y * (y < 0);
	x += ((lvl_width  - 1) - x) * (x >= lvl_width);
	y += ((lvl_height - 1) - y) * (y >= lvl_height);
	
	return tileset_data[x + (y * lvl_width)] & 0xFF;
}

unsigned int entity_physics(Entity *ent, int hit_mask) {
	
	
	// Get the sign (-/+) of the velocity components
	int sign_x = (ent->vel_x >> 31) | 1, sign_y = (ent->vel_y >> 31) | 1;
	int y_is_pos= -(~(ent->vel_y) >> 31); // If y is positive, equals 1, else 0;
	int y_is_neg = ent->vel_y >> 31; // If y is negative, equals -1, else 0;
	int x_is_pos= -(~(ent->vel_x) >> 31); // If x is positive, equals 1, else 0;
	int x_is_neg = ent->vel_x >> 31; // If x is negative, equals -1, else 0;
	
	// Box collision indexes - Tile values;
	int index = 0, index2 = 0;
	
	int top = FIXED2INT(ent->y),
		bot = top + ent->height - 1,
		lef = FIXED2INT(ent->x),
		rgt = lef + ent->width - 1;
	
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
			
			int shape = get_block(index, index2);
			if (!shape)
				continue;
			
			shape = tile_types[shape - 1];
			int type = (shape & TILE_TYPE_MASK) >> TILE_TYPE_SHIFT;
			int mask = 1 << (type - 1);
			
			if (!type || !(mask & hit_mask)) // If the block is 0 or if the block is not solid, ignore
				continue;
			
			shape = shape & TILE_SHAPE_MASK;
			
			int temp_offset;
			
			switch (shape) {
			
				fullshape_x:
			case SHAPE_FULL:
				temp_offset = (BLOCK2FIXED(index - x_is_neg) - INT2FIXED(ent->width * x_is_pos)) - ent->x;
				break;
			
			default:
				continue;
			}
			
			if (INT_ABS(temp_offset) < INT_ABS(offsetX)) { // If new movement is smaller, set collision data.
				offsetX = temp_offset; // Set offset
				hit_value_x = mask;
			}
			else if (temp_offset == offsetX)
			{
				hit_value_x |= mask;
			}
		}
		if (hit_value_x & hit_mask){
			ent->x += offsetX;
			offsetX += ent->vel_x;
			
			if (ent->vel_x != 0 && sign_x == INT_SIGN((BLOCK2FIXED(index) + 0x400) - (ent->x + (ent->width >> 1))))
				ent->vel_x = 0;
			else
				hit_value_x = 0;
			break;
		}
	}
	ent->x += ent->vel_x * !(hit_value_x & hit_mask);
	
	x_min = ent->x - x_is_neg * (INT2FIXED(ent->width) - 1);
	x_max = ent->x + x_is_pos * (INT2FIXED(ent->width) - 1);
	
	for (index = FIXED2BLOCK(y_min); index != FIXED2BLOCK(y_max + ent->vel_y) + sign_y; index += sign_y){
		for (index2 = FIXED2BLOCK(x_min); index2 != FIXED2BLOCK(x_max) + sign_x; index2 += sign_x){
			
			int shape = get_block(index2, index);
			if (!shape)
				continue;
			
			shape = tile_types[shape - 1];
			int type = (shape & TILE_TYPE_MASK) >> TILE_TYPE_SHIFT;
			int mask = 1 << (type - 1);
			
			if (!type || !(mask & hit_mask)) // If the block is 0 or if the block is not solid, ignore
				continue;
			
			shape = shape & TILE_SHAPE_MASK;
			
			int temp_offset;
			
			switch (shape) {
			
				fullshape_y:
			case SHAPE_FULL:
				temp_offset = (BLOCK2FIXED(index - y_is_neg) - INT2FIXED(ent->height * y_is_pos)) - ent->y;
				break;
				
			case SHAPE_PLATFORM:
				if (ent->vel_y < 0 ||
					bot > BLOCK2INT(index))
					continue;
					
				goto fullshape_y;
				break;
			default:
				continue;
			}
			
			if (INT_ABS(temp_offset) < INT_ABS(offsetY)) { // If new movement is smaller, set collision data.
				offsetY = temp_offset; // Set offset
				hit_value_y |= mask;
			}
			else if (temp_offset == offsetY)
			{
				hit_value_y |= mask;
			}
			
		}
		if (hit_value_y & hit_mask){
			ent->y += offsetY;
			offsetY += ent->vel_y;
			
			if (ent->vel_y != 0 && sign_y == INT_SIGN((BLOCK2FIXED(index) + 0x400) - (ent->y + (ent->height >> 1))))
				ent->vel_y = 0;
			else
				hit_value_y = 0;
			break;
		}
	}
	ent->y += ent->vel_y * !(hit_value_y & hit_mask);
	
	return (hit_value_x << 16) | hit_value_y;
}
unsigned int collide_rect(int x, int y, int width, int height, int hit_mask){
	int y_min = FIXED2INT(y),
		y_max = y_min + height - 1;
	
	int x_min = FIXED2INT(x),
		x_max = x_min + width - 1;
	
	// Block values that were hit - flag
	int blockValue;
	int hitValue = 0;
	int xCoor, yCoor;
	
	for (xCoor = INT2BLOCK(x_min); xCoor <= INT2BLOCK(x_max); ++xCoor){
		for (yCoor = INT2BLOCK(y_min); yCoor <= INT2BLOCK(y_max); ++yCoor){
			
			int shape = get_block(xCoor, yCoor);
			if (!shape)
				continue;
			
			shape = tile_types[shape - 1];
			int type = (shape & TILE_TYPE_MASK) >> TILE_TYPE_SHIFT;
			int mask = 1 << (type - 1);
			
			if (!type || !(mask & hit_mask)) // If the block is 0 or if the block is not solid, ignore
				continue;
			
			shape = shape & TILE_SHAPE_MASK;
			
			switch (shape) {
			
				fullshape:
			case SHAPE_FULL:
				hitValue |= mask;
				break;
				
			default:
				continue;
			}
		}
	}
	
	return hitValue;
}
unsigned int collide_entity(unsigned int ID) {
	int i = 0;
	
	Entity *id = &entities[ID], *other;
	
	int id_LX = FIXED2INT(id->x);
	int id_LY = FIXED2INT(id->y);
	int id_RX = id_LX + id->width - 1;
	int id_RY = id_LY + id->height - 1;
	int iter_LX, iter_LY, iter_RX, iter_RY;
	
	for (; i < ENTITY_LIMIT; ++i)
	{
		if (i == ID)
			continue;
		
		other = &entities[i];
		
		if (!ENT_FLAG(ACTIVE, i) || !ENT_FLAG(DETECT, i))
			continue;
		
		iter_LX = FIXED2INT(other->x);
		iter_LY = FIXED2INT(other->y);
		iter_RX = iter_LX + other->width - 1;
		iter_RY = iter_LY + other->height - 1;
		
		if (id_RX < iter_LX || iter_RX < id_LX || id_RY < iter_LY || iter_RY < id_LY)
			continue;
		
		return other->ID;
	}
	return 0xFFFFFFFF;
}
