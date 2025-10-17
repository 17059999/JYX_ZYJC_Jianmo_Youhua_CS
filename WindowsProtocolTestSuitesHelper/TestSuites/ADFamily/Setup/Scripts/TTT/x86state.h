//-----------------------------------------------------------------------------
//
// Nirvana
// Copyright (C) Microsoft Corporation.  All Rights Reserved.
//
// Description:
//
//    Nirvana processor state definitions for 32-bit X86 user mode
//
// Remarks:
//
//    This is a common header file for Nirvana clients and components
//
//-----------------------------------------------------------------------------

#pragma once

#ifndef X86STATE
#define X86STATE
#undef _SP
#undef _DI
// allow unnamed structs and unions
#pragma warning (push)
#pragma warning (disable:4201)

#ifndef CC_H2INC
#include <stddef.h> // for offsetof
#endif // CC_H2INC

#ifdef __cplusplus
namespace Nirvana
{
#endif


#pragma pack(4)

// x87/SSE/SSE2 multimedia registers are store in 512-byte FXSAVE format.
// This if for use on hosts that support SSE/SSE2, since FXSAVE is needed.

typedef struct XMMSTATE32
{
    unsigned short  _FCW;
    unsigned short  _FSW;
    unsigned short  _FTW;
    unsigned short  _FOP;
    unsigned long   _EIP;
    unsigned long   _CS;
    unsigned long   _DP;
    unsigned long   _DS;
    unsigned long   _MXCSR;
    unsigned long   _MXCSR_MASK;

    union
    {
        unsigned char   _ST0[16];
        struct
        {
            unsigned char   _MM0[8];
            unsigned char   _MM0_HIGH_UNUSED[8];
        };
    };
    union
    {
        unsigned char   _ST1[16];
        struct
        {
            unsigned char   _MM1[8];
            unsigned char   _MM1_HIGH_UNUSED[8];
        };
    };
    union
    {
        unsigned char   _ST2[16];
        struct
        {
            unsigned char   _MM2[8];
            unsigned char   _MM2_HIGH_UNUSED[8];
        };
    };
    union
    {
        unsigned char   _ST3[16];
        struct
        {
            unsigned char   _MM3[8];
            unsigned char   _MM3_HIGH_UNUSED[8];
        };
    };
    union
    {
        unsigned char   _ST4[16];
        struct
        {
            unsigned char   _MM4[8];
            unsigned char   _MM4_HIGH_UNUSED[8];
        };
    };
    union
    {
        unsigned char   _ST5[16];
        struct
        {
            unsigned char   _MM5[8];
            unsigned char   _MM5_HIGH_UNUSED[8];
        };
    };
    union
    {
        unsigned char   _ST6[16];
        struct
        {
            unsigned char   _MM6[8];
            unsigned char   _MM6_HIGH_UNUSED[8];
        };
    };
    union
    {
        unsigned char   _ST7[16];
        struct
        {
            unsigned char   _MM7[8];
            unsigned char   _MM7_HIGH_UNUSED[8];
        };
    };

    unsigned char   _XMM0[16];
    unsigned char   _XMM1[16];
    unsigned char   _XMM2[16];
    unsigned char   _XMM3[16];
    unsigned char   _XMM4[16];
    unsigned char   _XMM5[16];
    unsigned char   _XMM6[16];
    unsigned char   _XMM7[16];

    unsigned char   reserved[224];
} XMMSTATE32;

typedef struct XSAVEHEADER32
{
    ULONG64    XState_BV;
    ULONG64    reservedMustBeZero0;
    ULONG64    reservedMustBeZero1;
    unsigned char   reserved[40];
} XSAVEHEADER32;

typedef struct YMMSTATE32
{
    // high 128 bits for Ymm registers
    unsigned char   _YMMHIGH0[16];
    unsigned char   _YMMHIGH1[16];
    unsigned char   _YMMHIGH2[16];
    unsigned char   _YMMHIGH3[16];
    unsigned char   _YMMHIGH4[16];
    unsigned char   _YMMHIGH5[16];
    unsigned char   _YMMHIGH6[16];
    unsigned char   _YMMHIGH7[16];
    unsigned char   _YMMHIGH8[16];
    unsigned char   _YMMHIGH9[16];
    unsigned char   _YMMHIGH10[16];
    unsigned char   _YMMHIGH11[16];
    unsigned char   _YMMHIGH12[16];
    unsigned char   _YMMHIGH13[16];
    unsigned char   _YMMHIGH14[16];
    unsigned char   _YMMHIGH15[16];
} YMMSTATE32;

// The x87 floating point registers are store in 108-byte FNSAVE format.
// This is for hosts that only support x87 and MMX, but not SSE/SSE2.

typedef struct X87STATE32
{
    unsigned long   _FCW;
    unsigned long   _FSW;
    unsigned long   _FTW;
    unsigned long   _FIP;
    unsigned short  _CS;
    unsigned short  _OP;
    unsigned long   _DP;
    unsigned long   _DS;

    union
        {
        unsigned char       _sFPR[8*10];

        struct
            {
            unsigned char   _ST0[10];
            unsigned char   _ST1[10];
            unsigned char   _ST2[10];
            unsigned char   _ST3[10];
            unsigned char   _ST4[10];
            unsigned char   _ST5[10];
            unsigned char   _ST6[10];
            unsigned char   _ST7[10];
            };
        };
} X87STATE32;


// x86 descriptor
typedef struct _Descriptor32
{
    unsigned short pad[1];
    unsigned short limit;
    unsigned long base;
} Descriptor32;

// registers/state availabe only in kernel mode (eg CRs)
typedef struct _KernelRegs32
{
    unsigned long _CR0;
    unsigned long _CR2;
    unsigned long _CR3;
    unsigned long _CR4;

    unsigned long _DR0;
    unsigned long _DR1;
    unsigned long _DR2;
    unsigned long _DR3;
    union
    {
        unsigned long _DR4;
        unsigned long _DR6;
    };
    union
    {
        unsigned long _DR5;
        unsigned long _DR7;
    };

    Descriptor32 _GDTR;
    Descriptor32 _IDTR;

    unsigned short _TR;
    unsigned short _LDTR;

    unsigned long _SYSENTER_CS;
    void* _SYSENTER_ESP;
    void* _SYSENTER_EIP;

    char rsvd[8];       // for padding
} KernelRegs32;

// info about interrupts (TENTATIVE)
typedef struct _InterruptInfo32
{
    unsigned char vectorNo;
    unsigned char lengthINTn;       // instruction length for software "INT n"
                                    // 0 otherwise
    unsigned char iiPadding;
    unsigned char isException;      // non-zero value for exceptions
    unsigned long errorCode;
    unsigned long savedIP;
    unsigned long savedCS;
    unsigned long savedEFLAGS;
} InterruptInfo32;


// This is the user-mode x86 register state including integer, float, and SIMD regs

typedef struct X86REGS
{
    // the integer registers are in the same layout as a 486 task state segment

    union
        {
        unsigned long       _EIP;
        };

    union
        {
        unsigned long       _EFLAGS;
        unsigned short      _FLAGS;
        unsigned char       _FLAGS8;
        };

    union
        {
        unsigned long       _EAX;
        unsigned short      _AX;

        struct
            {
            unsigned char   _AL;
            unsigned char   _AH;
            };
        };

    union
        {
        unsigned long       _ECX;
        unsigned short      _CX;

        struct
            {
            unsigned char   _CL;
            unsigned char   _CH;
            };
        };

    union
        {
        unsigned long       _EDX;
        unsigned short      _DX;

        struct
            {
            unsigned char   _DL;
            unsigned char   _DH;
            };
        };

    union
        {
        unsigned long       _EBX;
        unsigned short      _BX;

        struct
            {
            unsigned char   _BL;
            unsigned char   _BH;
            };
        };

    union
        {
        unsigned long       _ESP;
        unsigned short      _SP;
        };

    union
        {
        unsigned long       _EBP;
        unsigned short      _BP;
        };

    union
        {
        unsigned long       _ESI;
        unsigned short      _SI;
        };

    union
        {
        unsigned long       _EDI;
        unsigned short      _DI;
        };

    unsigned long           _CSBase;
    unsigned long           _FSBase;
    unsigned long           _GSBase;

    unsigned short          _ES;
    unsigned short          _CS;
    unsigned short          _SS;
    unsigned short          _DS;
    unsigned short          _FS;
    unsigned short          _GS;

    // end of 486 task state segment layout

    // need to be 64 bytes aligned for xsave
    // we cannot use __declspec(align) because of H2Inc conversion, we use the static assert below to verify the alignment
    XMMSTATE32              _XMMREGS;

    // Avx extension:
    XSAVEHEADER32           _XSAVEHEADER;
    YMMSTATE32              _YMMREGS; // upper 128 bits of the YMM registers
} X86REGS;

#ifndef CC_H2INC
static_assert((offsetof(X86REGS, _XMMREGS) & 0xFFFFFFC0) == offsetof(X86REGS, _XMMREGS), "Static assert failed:  _XMMREGS is not 16 bytes aligned");

// Note that we have to use "sizeof(XMMSTATE32)" because we do not have any variable to use (and using the member directly is illegal)
static_assert(offsetof(X86REGS, _XMMREGS) + sizeof(XMMSTATE32) <= 1024, "The XMM state plus everything before it must fit in a 1K chunk");
#endif // CC_H2INC

// This is the kernel-mode x86 register state

typedef struct KX86REGS
{
    X86REGS      _UserRegs;
    KernelRegs32 _KernRegs;
} KX86REGS;


#pragma pack()

#ifdef __cplusplus
} // namespace
#endif

// allow unnamed structs and unions
#pragma warning (pop)

#endif // X86STATE

