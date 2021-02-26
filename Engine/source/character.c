#include "character.h"
#include "pixtro_basic.h"
#include "state_machine.h"

#define CHAR_WIDTH			12

#define CHAR_HEIGHT			24
#define CHAR_R_HEIGHT		12
#define HEIGHT_DIFF			12

#define JUMP_HEIGHT			-0x340
#define JUMP_ROLL_HEIGHT	-0x280

#define ROLL_SPEED			0x500
#define ROLL_JUMP			-0x140

#define HAS_FLAG(name, value)	(name##_FLAG & value)
#define SET_FLAG(name, value)	value |= name##_FLAG;
#define CLEAR_FLAG(name, value)	value &= ~name##_FLAG;

// flag 0 data
#define GROUND_FLAG			0x0001
#define HOLDJUMP_FLAG		0x0002
#define ROLLJUMP_FLAG		0x0004


// State definitions
#define STATE_NORMAL	0
#define STATE_ROLLING	1

StateMachine char_machine;

int roll_angle;

void roll_begin(int, int);
void roll_end(int, int);
int normal_update(int);
int roll_update(int);

int character_init(unsigned int* actor_index, unsigned char* data) {
	
	entities[*actor_index].ID |= ENT_ACTIVE_FLAG;
	entities[*actor_index].width = CHAR_WIDTH;
	entities[*actor_index].height = CHAR_HEIGHT;
	
	entities[*actor_index].flags[0] = 0;
	
	init_statemachine(&char_machine, 2);
	set_update(&char_machine, &normal_update, 0);
	
	set_update(&char_machine, &roll_update, 1);
	set_begin_state(&char_machine, &roll_begin, 1);
	set_end_state(&char_machine, &roll_end, 1);
	
	return 0;
}

void roll_begin(int index, int old_st) {
	
	load_sprite(SPR_char_roll, 0, SPRITE32x32);
	
	entities[index].y += INT2FIXED(HEIGHT_DIFF);
	entities[index].height = CHAR_R_HEIGHT;
	
	roll_angle = 0;
	
	CLEAR_FLAG(ROLLJUMP, entities[index].flags[0]);
}
void roll_end(int index, int new_st) {
	
	load_sprite(SPR_char_idle, 0, SPRITE32x32);
	
	entities[index].y -= INT2FIXED(HEIGHT_DIFF);
	entities[index].height = CHAR_HEIGHT;
}
int roll_update(int index) {
	Entity *ent = &entities[index];
	
	roll_angle += ent->vel_x >> 3;

	int dir = key_tri_horz();
	
	if (dir && HAS_FLAG(GROUND, ent->flags[0]))
		ent->vel_x = FIXED_APPROACH(ent->vel_x, (dir * ROLL_SPEED), 0x28);
	else
		ent->vel_x = FIXED_APPROACH(ent->vel_x, 0, 0x04);
	
	if (!KEY_DOWN_NOW(KEY_R) && !(collide_rect(ent->x, ent->y - INT2FIXED(HEIGHT_DIFF), ent->width, HEIGHT_DIFF, 0x1)))
		return STATE_NORMAL;
	
	
	return char_machine.state;
}
int normal_update(int index) {
	
	Entity *ent = &entities[index];
	if (INT_ABS(ent->vel_x) <= 0x200)
		CLEAR_FLAG(ROLLJUMP, ent->flags[0]);
	
	if (HAS_FLAG(GROUND, ent->flags[0])) {
		if (KEY_DOWN_NOW(KEY_R) && INT_ABS(ent->vel_x) > 0x200) {
			return STATE_ROLLING;
		}
		
		ent->vel_x = FIXED_APPROACH(ent->vel_x, (key_tri_horz() * 0x200), 0x80);
		
		if (key_pressed(KEY_A, 5)){
			
			if (KEY_DOWN_NOW(KEY_R)) {
				ent->vel_y = ROLL_JUMP;
				ent->vel_x = INT_SIGN(ent->vel_x) * ROLL_SPEED;
				SET_FLAG(ROLLJUMP, ent->flags[0]);
			}
			else {
				ent->vel_y = JUMP_HEIGHT;
				SET_FLAG(HOLDJUMP, ent->flags[0]);
			}
			
			clear_buffer(KEY_A);
		}
	}
	else {
		ent->vel_x = FIXED_APPROACH(ent->vel_x, (key_tri_horz() * 0x200), 0x24);
		
		if (ent->vel_y > 0)
			CLEAR_FLAG(HOLDJUMP, ent->flags[0]);
		
		if (HAS_FLAG(ROLLJUMP, ent->flags[0]) && key_pressed(KEY_A, 5)){
			
			ent->vel_y = JUMP_ROLL_HEIGHT;
			SET_FLAG(HOLDJUMP, ent->flags[0]);
			CLEAR_FLAG(ROLLJUMP, ent->flags[0]);
			
			clear_buffer(KEY_A);
		}
		
		if (HAS_FLAG(HOLDJUMP, ent->flags[0]) && !KEY_DOWN_NOW(KEY_A)) {
			
			ent->vel_y = FIXED_APPROACH(ent->vel_y, 0, 0x100);
		}
	}
	
	return char_machine.state;
}

void character_update(int index) {
		
	Entity *ent = &entities[index];
	ent->vel_y += 0x20;
	
	update_statemachine(&char_machine, index);
	
	int vel_y_prev = ent->vel_y;
	
	unsigned int hit_values = entity_physics(ent, 0x1);
	
	if (vel_y_prev > 0 && !ent->vel_y) {
		SET_FLAG(GROUND, ent->flags[0]);
		CLEAR_FLAG(HOLDJUMP, ent->flags[0]);
	}
	else {
		CLEAR_FLAG(GROUND, ent->flags[0]);
	}
}
void character_render(int index) {
	
	Entity ent = entities[index];
	
	switch (char_machine.state) {
		case STATE_NORMAL:
			draw(ent.x - 0xA00, ent.y - 0x800, 0, 0, 0, 0);
		break;
		case STATE_ROLLING: {
			AffineMatrix matrix = matrix_identity();
			TRANSLATE_MATRIX(matrix, 0, -0x700);
			ROTATE_MATRIX(matrix, roll_angle >> 2);
			TRANSLATE_MATRIX(matrix, ent.x + 0x600, ent.y + 0x600);
			
			draw_affine(matrix, 0, 0, 0);
			}
		break;
	}
}