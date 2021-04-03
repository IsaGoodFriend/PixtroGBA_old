	
	.align	4
	.globl	load_header
	.code	32
	
@ void load_header(unsigned char* ptr)
@ r0 is always the level data pointer
load_header:
	
	@ --- Get width and height values ---
	ldr		r1, =lvl_width
	ldrh	r2, [r0]
	str		r2, [r1]
	ldrh	r2, [r0, #2]
	str		r2, [r1, #4]
	
	add		r0,	#4
	
	@ --- Load metadata ---
	ldr		r1,	=level_meta	@ get meta data array
.ld_meta:
	ldr		r2,	[r0]		@ load meta index
	
	cmp		r2, #128
	bge		.ld_meta_e		@ skip if >= 128
	
	ldr		r3,	[r0, #1]	@ load meta value
	str		r3,	[r1, r2]	@ save meta value
	
	add		r0,	#2
.ld_meta_e:
	add		r0, #1
	
	
	@ --- end of loading ---
	ldr		r1, =lvl_info
	str		r0, [r1]
	
	bx		lr


	.align	4
	.globl	load_midground
	.code	32

@ void load_midground(int index)
load_midground:
	
	ldr		r1,	=tileset_data	@r1 = tileset_data[0x2000 * index]
	mov		r2, #0x2000
	mul		r3, r2, r1
	mov		r2, r3
	add		r1, r1, r2
	
	ldr		r0, =lvl_info
	ldr		r0, [r0]			@r0 = lvl_info
	
	ldrb	r2, [r0]
	ldrb	r3, [r0]
	
	bx		lr