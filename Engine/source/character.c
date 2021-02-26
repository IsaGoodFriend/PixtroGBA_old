#include "character.h"
#include "pixtro_basic.h"

int character_init(unsigned int* actor_index, unsigned char* data) {
	
	return 0;
}
void character_update(int index) {
	
	Entity *ent = &entities[index];
	ent->vel_y += 0x20;
	ent->vel_x = FIXED_APPROACH(ent->vel_x, (key_tri_horz() * 0x200), 0x40);
	
	
	if (key_pressed(KEY_A, 20)){
		ent->vel_y = -0x280;
		clear_buffer(KEY_A);
	}
	
	entity_physics(ent, 0x1);
	
}
void character_render(int index) {
	
	Entity ent = entities[index];
	
	draw(ent.x - 0x200, ent.y, 0, 0, 0, 0);
}