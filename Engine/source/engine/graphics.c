#include <tonc.h>
#include <string.h>

#include "graphics.h"
#include "core.h"

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
int affine_count;

#define BANK_LIMIT			64
#define BANK_MEM_START		0x60

// Max of 128 sprites
#define SPRITE_LIMIT		128

// Sprite bank information
int shapes[BANK_LIMIT], indexes[BANK_LIMIT], ordered[BANK_LIMIT];

unsigned int *anim_bank[BANK_LIMIT], anim_meta[BANK_LIMIT];

OBJ_ATTR obj_buffer[SPRITE_LIMIT];
OBJ_ATTR *sprite_pointer;
OBJ_AFFINE *obj_aff_buffer= (OBJ_AFFINE*)obj_buffer;


void load_sprite(unsigned int *_sprite, int _index, int _shape) {

	anim_bank[_index] = NULL;
	
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
void load_anim_sprite(unsigned int *sprites, int index, int shape, int frames, int speed) {
	load_sprite(sprites, index, shape);
	
	anim_bank[index] = sprites;
	
	speed &= 0xF;
	speed = (speed) & 0xF;
	if (frames)
		frames = (frames - 1) & 0xF;
	
	anim_meta[index] = speed;
	anim_meta[index] |= speed << 4;
	
	anim_meta[index] |= frames << 8;
	anim_meta[index] |= frames << 12;
	
	anim_meta[index] |= shape << 16;
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

void draw_affine_big(AffineMatrix _matrix, int _sprite, int _prio, int _pal) {
	if (affine_count == 32)
		return;
	
	int x = FIXED2INT(_matrix.values[2]), y = FIXED2INT(_matrix.values[5]);
	
	if (drawing_flags & DFLAG_CAM_FOLLOW)
	{
		x -= cam_x - 120;
		y -= cam_y - 80;
	}
	
	int shape = shapes[_sprite];
	
	x -= shape_width[shape];
	y -= shape_height[shape];
	
	obj_set_attr(sprite_pointer,
	((shape & 0xC) << 12) | SPRITE_Y(y) | ATTR0_AFF_DBL,
	((shape & 0x3) << 14) | SPRITE_X(x) | ATTR1_AFF_ID(affine_count),
	ATTR2_PALBANK(_pal) | ATTR2_PRIO(_prio) | (indexes[_sprite]));
	
	int det = FIXED_MULT(_matrix.values[0], _matrix.values[4]) -
		FIXED_MULT(_matrix.values[1], _matrix.values[3]);
	
	if (det)
		det = FIXED_DIV(0x100, det);
	
	obj_aff_set((obj_aff_buffer + affine_count),
		FIXED_MULT(_matrix.values[4], det),
		FIXED_MULT(_matrix.values[3], det),
		FIXED_MULT(_matrix.values[1], det),
		FIXED_MULT(_matrix.values[0], det));
	
	++sprite_pointer;
	++sprite_count;
	++affine_count;
}
void draw_affine(AffineMatrix _matrix, int _sprite, int _prio, int _pal) {
	
	if (affine_count == 32)
		return;
	
	AffineMatrix transform = matrix_multiply(matrix_identity(), _matrix);
	
	int x = FIXED2INT(transform.values[2]), y = FIXED2INT(transform.values[5]);
	
	if (drawing_flags & DFLAG_CAM_FOLLOW)
	{
		x -= cam_x - 120;
		y -= cam_y - 80;
	}
	
	int shape = shapes[_sprite];
	
	x -= shape_width[shape] >> 1;
	y -= shape_height[shape] >> 1;
	
	obj_set_attr(sprite_pointer,
	((shape & 0xC) << 12) | SPRITE_Y(y) | ATTR0_AFF,
	((shape & 0x3) << 14) | SPRITE_X(x) | ATTR1_AFF_ID(affine_count),
	ATTR2_PALBANK(_pal) | ATTR2_PRIO(_prio) | (indexes[_sprite]));
	
	int det = FIXED_MULT(_matrix.values[0], _matrix.values[4]) -
		FIXED_MULT(_matrix.values[1], _matrix.values[3]);
	
	if (det)
		det = FIXED_DIV(0x100, det);
	
	obj_aff_set((obj_aff_buffer + affine_count),
		FIXED_MULT(_matrix.values[4], det),
		FIXED_MULT(_matrix.values[3], det),
		FIXED_MULT(_matrix.values[1], det),
		FIXED_MULT(_matrix.values[0], det));
	
	++sprite_pointer;
	++sprite_count;
	++affine_count;
}
void draw(int _x, int _y, int _sprite, int _flip, int _prio, int _pal) {
	
	_x = FIXED2INT(_x);
	_y = FIXED2INT(_y);
	
	if (drawing_flags & DFLAG_CAM_FOLLOW)
	{
		_x -= cam_x - 120;
		_y -= cam_y - 80;
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
	int i;
	
	for (i = 0; i < BANK_LIMIT; ++i) {
		if (!anim_bank[i])
			continue;
		
		anim_meta[i]--;
		
		if (!(anim_meta[i] & 0xF)) {
			// Reset counter
			anim_meta[i] |= (anim_meta[i] & 0xF0) >> 4;
			
			// Update the frame index
			anim_meta[i] = ((anim_meta[i] + 0x100) & 0xF00) + (anim_meta[i] & ~0xF00);
			
			// Reset the frame index
			if ((anim_meta[i] & 0xF00) > ((anim_meta[i] & 0xF000) >> 4)) {
				anim_meta[i] &= ~0xF00;
			}
			
			unsigned int *ptr = anim_bank[i];
			int offset = (anim_meta[i] & 0xF00) >> 8;
			int shape = (anim_meta[i] & 0xF0000) >> 16;
			
			load_sprite(&anim_bank[i][(offset * (shape2size[shape] >> 2))], i, shape);
			
			anim_bank[i] = ptr;
		}
		
	}
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
	affine_count = 0;
	
	oam_copy(oam_mem, obj_buffer, SPRITE_LIMIT);
	
	sprite_pointer = (OBJ_ATTR*)&obj_buffer;
}