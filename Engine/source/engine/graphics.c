#include <tonc.h>
#include <string.h>

#include "graphics.h"
#include "core.h"
#include "math.h"

#define copyTile			32
#define copyPalette			32

#define tileSize			8

const int shape2size[12] = {
	copyTile,
	copyTile * 4,
	copyTile * 16,
	copyTile * 64,
	
	copyTile * 2,
	copyTile * 4,
	copyTile * 8,
	copyTile * 32,
	
	copyTile * 2,
	copyTile * 4,
	copyTile * 8,
	copyTile * 32,
};
const int shape_width[12] = {
	tileSize,
	tileSize * 2,
	tileSize * 4,
	tileSize * 8,
	
	tileSize * 2,
	tileSize * 4,
	tileSize * 4,
	tileSize * 8,
	
	tileSize,
	tileSize,
	tileSize * 2,
	tileSize * 4,
};
const int shape_height[12] = {
	tileSize,
	tileSize * 2,
	tileSize * 4,
	tileSize * 8,
	
	tileSize,
	tileSize,
	tileSize * 2,
	tileSize * 4,
	
	tileSize * 2,
	tileSize * 4,
	tileSize * 4,
	tileSize * 8,
};

int drawing_flags = DFLAG_CAM_FOLLOW;

int cam_x, cam_y, prev_cam_x, prev_cam_y;

#define SPRITE_X(n)			((n & ATTR1_X_MASK)<<ATTR1_X_SHIFT)
#define SPRITE_Y(n)			((n & ATTR0_Y_MASK)<<ATTR0_Y_SHIFT)
int sprite_count, prev_sprite_count;

#define BANK_LIMIT			64
#define BANK_MEM_START		0x60

// Max of 128 sprites
#define SPRITE_LIMIT		128

// Sprite bank information
int shapes[BANK_LIMIT], indexes[BANK_LIMIT], ordered[BANK_LIMIT];

unsigned int *anims[BANK_LIMIT];

OBJ_ATTR obj_buffer[SPRITE_LIMIT];
OBJ_ATTR *sprite_pointer;
OBJ_AFFINE *obj_aff_buffer= (OBJ_AFFINE*)obj_buffer;


void load_sprite(unsigned short *_sprite, int _index, int _shape) {
	
	int bankLoc, size = shape2size[_shape];
	
	// If there's a sprite already loaded here, and it's the same size or bigger, replace it
	if (shapes[_index] >= 0 && shape2size[shapes[_index]] >= size) {
		bankLoc = indexes[_index];
	}
	else {
		int i;
		bankLoc = BANK_MEM_START;
		
		//Search for an open spot in the sprites
		for (i = 0; i < BANK_LIMIT; ++i) {
			
			int diff = indexes[ordered[i + 1]] - indexes[ordered[i]];
			
			if (shapes[ordered[i]] >= 0) {
				diff -= shape2size[shapes[ordered[i]]] >> 5;
			}
			
			if (diff >= size >> 5) {
				bankLoc = indexes[ordered[i]];
							
				if (shapes[ordered[i]] >= 0)
					bankLoc += shape2size[shapes[ordered[i]]] >> 5;
				
				break;
			}
		}
		
		bool swapping = false;
		
		int ind;
		for (ind = BANK_LIMIT - 2; ind >= 0; --ind) {
			if (ind == i)
				break;
			
			if (ordered[i + 1] == _index)
				swapping = true;
			
			if (swapping) {
				int t = ordered[i];
				ordered[i] = ordered[i + 1];
				ordered[i + 1] = t;
			}
			
		}
		
		indexes[_index] = bankLoc;
	}
	
	shapes[_index] = _shape;
	
	memcpy(&tile_mem[4][bankLoc], _sprite, size);
}
void load_anim_sprite(unsigned short *_sprites, int _index, int _frames, int _shape) {
	load_sprite(_sprites, _index, _shape);
}
void load_tileset(const unsigned short *_tiles, int _count) {
	memcpy(&tile_mem[FG_TILESET][1], _tiles, _count << 5);
}
void load_obj_pal(unsigned short *_pal, int _palIndex) {
	memcpy(&pal_obj_mem[_palIndex << 4], _pal, copyPalette);
}
void load_bg_pal(unsigned short *_pal, int _palIndex) {
	memcpy(&pal_bg_mem[_palIndex << 4], _pal, copyPalette);
}

void draw(int _x, int _y, int _sprite, int _flip, int _prio, int _pal) {
	
	_x = FIXED2INT(_x);
	_y = FIXED2INT(_y);
	
	if (drawing_flags & DFLAG_CAM_FOLLOW)
	{
		_x -= cam_x;
		_y -= cam_y;
	}
	
	int shape = shapes[_sprite];
	
	if (_x + shape_width [shape] <= 0 || _x > 240 ||
		_y + shape_height[shape] <= 0 || _y > 160)
		return;
	
	obj_set_attr(sprite_pointer,
	((shape & 0xC) << 12) | SPRITE_Y(_y),
	((shape & 0x3) << 14) | SPRITE_X(_x) | _flip,
	ATTR2_PALBANK(_pal) | ATTR2_PRIO(_prio) | (indexes[_sprite]));
	
	++sprite_pointer;
	++sprite_count;
}

void update_anims() {
	
}

void init_drawing() {
	oam_init(obj_buffer, SPRITE_LIMIT);
	sprite_pointer = (OBJ_ATTR*)&obj_buffer;
	int i;
	
	indexes[0] = BANK_MEM_START;
	shapes[0] = -1;
	for (i = 1; i < BANK_LIMIT; ++i) {
		indexes[i] = 0x8000;
		ordered[i] = i;
		shapes[i] = -1;
	}
}
void end_drawing() {
	
	if (sprite_count < prev_sprite_count){
		int i = sprite_count;
		for (; i < prev_sprite_count; ++i)
		{
			sprite_pointer->attr0 = 0x0200;
			++sprite_pointer;
		}
	}
	prev_sprite_count = sprite_count;
	sprite_count = 0;
	
	oam_copy(oam_mem, obj_buffer, SPRITE_LIMIT);
	
	sprite_pointer = (OBJ_ATTR*)&obj_buffer;
}