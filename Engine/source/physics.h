#ifndef _PIX_PHYSICS
#define _PIX_PHYSICS

#include "core.h"

extern unsigned char yShift;
extern unsigned int width, height;
extern unsigned char collisionData[4096];

extern unsigned int collide_char(Actor *actor, int hitMask, int detectMask);
//extern unsigned int collide_rect(int x, int y, int width, int height);
//extern unsigned int collide_entity(unsigned int ID);

int GetActorX(int value, int camera);
int GetActorY(int value, int camera);

#endif
