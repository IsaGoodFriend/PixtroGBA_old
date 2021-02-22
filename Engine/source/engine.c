#include "engine.h"
#include <tonc.h>

#include "pixtro_basic.h"

#include "levels.h"
#include "load_data.h"

void update();
void render();

extern unsigned int tileTypes[128];

void test_update(int index) {
	Entity *ent = &entities[index];
	ent->velY += 0x20;
	ent->velX = FIXED_APPROACH(ent->velX, (key_tri_horz() * 0x200), 0x40);
	
	
	if (key_pressed(KEY_A, 20)){
		ent->velY = -0x280;
		clear_buffer(KEY_A);
	}
	
	entity_physics(ent, 0x1, 0x1);
	
}
void test_render(int index) {
	Entity ent = entities[index];
	
	AffineMatrix mat = matrix_identity();
	
	TRANSLATE_MATRIX(mat, 0x000, -0x800);
	SCALE_MATRIX(mat, 0x100);
	ROTATE_MATRIX(mat, -(ent.velX / 14));
	TRANSLATE_MATRIX(mat, ent.x + INT2FIXED(ent.width >> 1), ent.y + INT2FIXED(ent.height));
	
	draw_affine(mat, 0, 0, 0);
	
	//draw(ent.x - 0x200, ent.y, 0, 0, 0, 0);
}

void on_update() {
	cam_x = FIXED2INT(entities[0].x);
	cam_y = FIXED2INT(entities[0].y);
}

void init() {
	
	tileTypes[0] = 0x10;
	
	entity_update[0] = &test_update;
	entity_render[0] = &test_render;
	
	LOAD_TILESET(test);
	
	load_bg_pal(PAL_test, 0);
	load_obj_pal(PAL_test, 0);
	
	load_sprite(&SPR_test_anim[0], 0, SPRITE16x16);
	
	set_layer_priority(1, 1);
	set_layer_priority(2, 2);
	set_layer_priority(3, 3);
	
	LOAD_BG(sample_ase, 1);
	
	set_foreground_count(1);
	finalize_layers();
	
	load_collision((unsigned char*)LVL_test);
	load_midground(0);
	load_entities();
	reset_cam();
	
	custom_update = &on_update;
	
	entities[0].ID |= ENT_ACTIVE_FLAG;
	entities[0].width = 12;
	entities[0].height = 16;
}

void init_settings() {
	
}