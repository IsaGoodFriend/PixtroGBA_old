	
	.align	4
	.globl	load_header
	.code	32
@ void load_header(unsigned char* ptr)
@ r0 is always the level data pointer
load_header:
	
	@ --- Get width and height values ---
	ldr		r1, =lvl_width	@ get lvl_width pointer
	ldrh	r2, [r0]		@ set lvl_width value
	str		r2, [r1]		@ set lvl_width value
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
	
	add		r0,	#2			@ move to next meta index value
	b		.ld_meta
.ld_meta_e:
	add		r0, #1
	
	@ --- end of loading ---
	ldr		r1, =lvl_info
	str		r0, [r1]
	bx		lr
@ end load_header


	.align	4
	.globl	load_midground
	.code	32
@ void load_midground(int index)
load_midground:
	
	push	{lr}

	ldr		r1,	=tileset_data	@ r1 = &tileset_data[0]
	ldr		r1,	[r1]
	mov		r2, #0x2000
	mul		r3, r2, r0			@ r3 = 0x2000 * index
	add		r1, r1, r3			@ r1 = &tileset_data[0x2000 * index]

	ldr		r2, =lvl_width
	ldr		r0, [r2]
	ldr		r2, [r2, #4]
	mul		lr, r0, r2			@ lr = lvl_width * lvl_height;
	
	ldr		r0, =lvl_info
	ldr		r0, [r0]			@ r0 = lvl_info (get the pointer in lvl_info, not the pointer to lvl_info itself)
	
.ld_mid:
	ldrb	r2, [r0]			@ r2 = count
	add		r0, r0, #1
	ldrh	r3, [r0]			@ r3 = value
	add		r0, r0, #2

	@ while (r2 != 0) strh; r2--
.ld_mid_store:
	strh	r3, [r1]
	add		r1, #2

	sub		lr, #1
	cmp		lr, #0
	beq		.ld_mid_end			@ if --lr == 0, finish loading

	sub		r2, #1
	cmp		r2, #0
	bne		.ld_mid_store

	b		.ld_mid
	
.ld_mid_end:
	@ End of the function
	ldr		r1, =lvl_info		@ store lvl_info value back to pointer
	str		r0, [r1]
	pop		{lr}
	bx		lr