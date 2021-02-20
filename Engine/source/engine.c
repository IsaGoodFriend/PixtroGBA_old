#include "engine.h"
#include <tonc.h>

#include "core.h"
#include "graphics.h"
#include "levels.h"
#include "physics.h"
#include "particles.h"
#include "load_data.h"

void update();
void render();

extern unsigned int tileTypes[128];

void test_update(int index) {
	Entity *ent = &entities[index];
	ent->velY += 0x20;
	ent->velX = key_tri_horz() * 0x180;
	
	if (key_hit(KEY_A)){
		ent->velY = -0x280;
	}
	
	entity_physics(ent, 0x1, 0x1);
	
}
void test_render(int index) {
	Entity ent = entities[index];
	
	draw(ent.x - 0x200, ent.y, 0, 0, 0, 0);
}

void on_update() {
	
}

void init() {
	
	tileTypes[0] = 0x10;
	
	entity_update[0] = &test_update;
	entity_render[0] = &test_render;
	
	LOAD_TILESET(test);
	
	load_bg_pal(PAL_test, 0);
	load_obj_pal(PAL_test, 0);
	
	load_anim_sprite(&SPR_test_anim[0], 0, SPRITE16x16, 4, SPR_test_anim_len);
	
	set_layer_priority(1, 1);
	set_layer_priority(2, 2);
	set_layer_priority(3, 3);
	
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