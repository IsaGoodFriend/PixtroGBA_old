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

int drawing_flags;

int cam_x, cam_y, prev_cam_x, prev_cam_y;

int bg0x_follow, bg0y_follow, bg1x_follow, bg1y_follow, bg2x_follow, bg2y_follow, bg3x_follow, bg3y_follow;

#define SPRITE_X(n)			((n & ATTR1_X_MASK)<<ATTR1_X_SHIFT)
#define SPRITE_Y(n)			((n & ATTR0_Y_MASK)<<ATTR0_Y_SHIFT)
int spriteCount, prevSpriteCount;

#define BANK_LIMIT		64

// Sprite bank information
int shapes[BANK_LIMIT], indexes[BANK_LIMIT], ordered[BANK_LIMIT];

unsigned int *anims[BANK_LIMIT];

OBJ_ATTR obj_buffer[128];
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
		bankLoc = 0;
		
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
void load_obj_palette(unsigned short *_pal, int _palIndex) {
	memcpy(&pal_obj_mem[_palIndex << 4], _pal, copyPalette);
}
void load_bg_palette(unsigned short *_pal, int _palIndex) {
	memcpy(&pal_bg_mem[_palIndex << 4], _pal, copyPalette);
}

void draw(int _x, int _y, int _sprite, int _flip, int _prio, int _pal) {
	
	_x = FIXED2INT(_x);
	_y = FIXED2INT(_y);
	
	if (drawing_flags & DFLAG_CAM)
	{
		_x -= cam_x;
		_y -= cam_y;
	}
	
	int shape = shapes[_sprite];
	
	if (_x + shape_width[shape] <= 0)
		return;
	
	obj_set_attr(sprite_pointer,
	((shape & 0xC) << 12) | SPRITE_Y(_y),
	((shape & 0x3) << 14) | SPRITE_X(_x) | _flip,
	ATTR2_PALBANK(_pal) | ATTR2_PRIO(_prio) | (indexes[_sprite]));
	
	++sprite_pointer;
	++spriteCount;
}

void update_anims() {
	
}

void init_drawing() {
	oam_init(obj_buffer, 128);
	sprite_pointer = (OBJ_ATTR*)&obj_buffer;
	int i;
	
	shapes[0] = -1;
	for (i = 1; i < BANK_LIMIT; ++i) {
		indexes[i] = 0x8000;
		ordered[i] = i;
		shapes[i] = -1;
	}
}
void end_drawing() {
	
	if (spriteCount < prevSpriteCount){
		int i = spriteCount;
		for (; i < prevSpriteCount; ++i)
		{
			sprite_pointer->attr0 = 0x0200;
			++sprite_pointer;
		}
	}
	prevSpriteCount = spriteCount;
	spriteCount = 0;
	
	oam_copy(oam_mem, obj_buffer, 128);
	
	sprite_pointer = (OBJ_ATTR*)&obj_buffer;
}

void update_camera() {
	
	REG_BG0HOFS = INT2FIXED(FIXED_MULT(INT2FIXED(cam_x), bg0x_follow));
	REG_BG0VOFS = INT2FIXED(FIXED_MULT(INT2FIXED(cam_y), bg0y_follow));
	REG_BG1HOFS = INT2FIXED(FIXED_MULT(INT2FIXED(cam_x), bg1x_follow));
	REG_BG1VOFS = INT2FIXED(FIXED_MULT(INT2FIXED(cam_y), bg1y_follow));
	REG_BG2HOFS = INT2FIXED(FIXED_MULT(INT2FIXED(cam_x), bg2x_follow));
	REG_BG2VOFS = INT2FIXED(FIXED_MULT(INT2FIXED(cam_y), bg2y_follow));
	REG_BG3HOFS = INT2FIXED(FIXED_MULT(INT2FIXED(cam_x), bg3x_follow));
	REG_BG3VOFS = INT2FIXED(FIXED_MULT(INT2FIXED(cam_y), bg3y_follow));
	
}