#include "engine.h"
#include <tonc.h>

#include "pixtro_basic.h"

#include "levels.h"
#include "load_data.h"

void update();
void render();

extern unsigned int tile_types[128];

void test_update(int index) {
	Entity *ent = &entities[index];
	ent->vel_y += 0x20;
	ent->vel_x = FIXED_APPROACH(ent->vel_x, (key_tri_horz() * 0x200), 0x40);
	
	
	if (key_pressed(KEY_A, 20)){
		ent->vel_y = -0x280;
		clear_buffer(KEY_A);
	}
	
	entity_physics(ent, 0x1, 0x1);
	
}
void test_render(int index) {
	Entity ent = entities[index];
	
	draw(ent.x - 0x200, ent.y, 0, 0, 0, 0);
}

void on_update() {
	cam_x = FIXED2INT(entities[0].x);
	cam_y = FIXED2INT(entities[0].y);
}

void init() {
	
	SET_TILE_DATA(0, SHAPE_FULL, 0);
	
	entity_update[0] = &test_update;
	entity_render[0] = &test_render;
	
	LOAD_TILESET(test);
	
	load_bg_pal(PAL_test, 0);
	load_obj_pal(PAL_test, 0);
	
	load_sprite(&SPR_test_anim[0], 0, SPRITE16x16);
	
	LOAD_BG(sample_ase, 1);
	
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