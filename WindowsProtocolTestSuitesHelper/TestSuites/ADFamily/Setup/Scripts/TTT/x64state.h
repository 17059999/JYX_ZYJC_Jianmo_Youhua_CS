//-----------------------------------------------------------------------------
//
// Nirvana
// Copyright (C) Microsoft Corporation.  All Rights Reserved.
//
// Description:
//
//    Nirvana processor state definitions for 64-bit X64 user mode
//
// Remarks:
//
//    This is a common header file for Nirvana clients and components
//
//-----------------------------------------------------------------------------

#pragma once

#ifndef CC_H2INC
#include <windows.h>
#endif

#ifndef CC_H2INC
#include <stddef.h> // for offsetof
#endif // CC_H2INC

#ifndef X64STATE
#define X64STATE
#undef _SP
#undef _DI
#ifdef __cplusplus
namespace Nirvana
{
#endif

#pragma warning (push)
#pragma warning (disable:4201)
#pragma pack(4)

//
// X64 data structures used by Nirvava Reference Emulator
//
// x87/SSE/SSE2 multimedia registers are store in 512-byte FXSAVE format.
// This if for use on hosts that support SSE/SSE2, since FXSAVE is needed.

typedef struct XMMSTATE64
{
    unsigned short  _FCW;
    unsigned short  _FSW;
    unsigned short  _FTW;
    unsigned short  _FOP;
    union
    {
        ULONG64     _RIP;
        struct
        {
            unsigned long   _EIP;
            unsigned long   _CS;
        };
    };
    union
    {
        ULONG64     _DP64;
        struct
        {
            unsigned long   _DP;
            unsigned long   _DS;
        };
    };
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
    unsigned char   _XMM8[16];
    unsigned char   _XMM9[16];
    unsigned char   _XMM10[16];
    unsigned char   _XMM11[16];
    unsigned char   _XMM12[16];
    unsigned char   _XMM13[16];
    unsigned char   _XMM14[16];
    unsigned char   _XMM15[16];

    unsigned char   reserved[96];
} XMMSTATE64;

typedef struct XSAVEHEADER64
{
    ULONG64         XState_BV;
    ULONG64         reservedMustBeZero0;
    ULONG64         reservedMustBeZero1;
    unsigned char   reserved[40];
} XSAVEHEADER64;

typedef struct YMMSTATE64
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
} YMMSTATE64;


// The x87 floating point registers are store in 108-byte FNSAVE format.
// This is for hosts that only support x87 and MMX, but not SSE/SSE2.

typedef struct X87STATE64
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
} X87STATE64;


// x64 descriptor
typedef struct _Descriptor64
{
    unsigned short pad[3];
    unsigned short limit;
    ULONG64 base;
} Descriptor64;

// registers/state availabe only in kernel mode (eg CRs)
typedef struct _KernelRegs64
{
    ULONG64 _CR0;
    ULONG64 _CR2;
    ULONG64 _CR3;
    ULONG64 _CR4;

    ULONG64 _DR0;
    ULONG64 _DR1;
    ULONG64 _DR2;
    ULONG64 _DR3;
    union
    {
        ULONG64 _DR4;
        ULONG64 _DR6;
    };
    union
    {
        ULONG64 _DR5;
        ULONG64 _DR7;
    };

    Descriptor64 _GDTR;
    Descriptor64 _IDTR;

    unsigned short _TR;
    unsigned short _LDTR;

    char rsvd[4];       // for padding

    ULONG64 _EFER;                  // Extended Feature Enable Register
    ULONG64 _STAR;                  // Syscal/sysret Target Address Register
    void*   _LSTAR;                 // Long-mode STAR
    void*   _CSTAR;                 // Compatibility STAR
    ULONG64 _SFMASK;                // Syscall Flags Mask

    ULONG64 _GsBaseSwap;            // from MSR_KernelGsBase

    ULONG64 _CR8;
} KernelRegs64;

// info about interrupts (TENTATIVE)
typedef struct _InterruptInfo64
{
    unsigned long errorCode;
    unsigned char vectorNo;
    unsigned char lengthINTn;       // instruction length for software "INT n"
                                    // 0 otherwise
    unsigned char iiPadding;
    unsigned char isException;      // non-zero value for exceptions
    ULONG64 savedIP;
    ULONG64 savedCS;
    ULONG64 savedEFLAGS;
    ULONG64 savedSP;
} InterruptInfo64;


// This is the user-mode x64 register state including integer, float, and SIMD regs

typedef struct X64REGS
{
    // the integer registers are in the same layout as a 486 task state segment

    union
        {
        ULONG64             _RIP;
        unsigned long       _EIP;
        };

    union
        {
        ULONG64             _RFLAGS;
        unsigned long       _EFLAGS;
        unsigned short      _FLAGS;
        unsigned char       _FLAGS8;
        };

    union
        {
        ULONG64             _RAX;
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
        ULONG64             _RCX;
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
        ULONG64             _RDX;
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
        ULONG64             _RBX;
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
        ULONG64             _RSP;
        unsigned long       _ESP;
        unsigned short      _SP;
        unsigned char       _SPL;
       };

    union
        {
        ULONG64             _RBP;
        unsigned long       _EBP;
        unsigned short      _BP;
        unsigned char       _BPL;
        };

    union
        {
        ULONG64             _RSI;
        unsigned long       _ESI;
        unsigned short      _SI;
        unsigned char       _SIL;
        };

    union
        {
        ULONG64             _RDI;
        unsigned long       _EDI;
        unsigned short      _DI;
        unsigned char       _DIL;
        };

    union
        {
        ULONG64             _R8;
        unsigned long       _R8D;
        unsigned short      _R8W;
        unsigned char       _R8B;
        };

    union
        {
        ULONG64             _R9;
        unsigned long       _R9D;
        unsigned short      _R9W;
        unsigned char       _R9B;
        };

    union
        {
        ULONG64             _R10;
        unsigned long       _R10D;
        unsigned short      _R10W;
        unsigned char       _R10B;
        };

    union
        {
        ULONG64             _R11;
        unsigned long       _R11D;
        unsigned short      _R11W;
        unsigned char       _R11B;
        };

    union
        {
        ULONG64             _R12;
        unsigned long       _R12D;
        unsigned short      _R12W;
        unsigned char       _R12B;
        };

    union
        {
        ULONG64             _R13;
        unsigned long       _R13D;
        unsigned short      _R13W;
        unsigned char       _R13B;
        };

    union
        {
        ULONG64             _R14;
        unsigned long       _R14D;
        unsigned short      _R14W;
        unsigned char       _R14B;
        };

    union
        {
        ULONG64             _R15;
        unsigned long       _R15D;
        unsigned short      _R15W;
        unsigned char       _R15B;
        };

    ULONG64                 _FSBase;
    ULONG64                 _GSBase;

    unsigned short          _ES;
    unsigned short          _CS;
    unsigned short          _SS;
    unsigned short          _DS;
    unsigned short          _FS;
    unsigned short          _GS;

    // end of 486 task state segment layout

    unsigned char           reserved[20];    // 64-byte alignment for xsave

    // we cannot use __declspec(align) because of H2Inc conversion, we use the static assert below to verify the alignment
    XMMSTATE64              _XMMREGS;

    // Avx extension:
    XSAVEHEADER64           _XSAVEHEADER;
    YMMSTATE64              _YMMREGS; // upper 128 bits of the YMM registers
} X64REGS;

#ifndef CC_H2INC
static_assert((offsetof(X64REGS, _XMMREGS) & 0xFFFFFFC0) == offsetof(X64REGS, _XMMREGS), "Static assert failed:  _XMMREGS is not 64 bytes aligned");

// Note that we have to use "sizeof(XMMSTATE64)" because we do not have any variable to use (and using the member directly is illegal)
static_assert((offsetof(X64REGS, _XMMREGS) + sizeof(XMMSTATE64)) <= 1024, "The XMM state plus everything before it must fit in a 1K chunk");
#endif // CC_H2INC

// This is the kernel-mode x64 register state
typedef struct KX64REGS
{
    X64REGS      _UserRegs;
    KernelRegs64 _KernRegs;
} KX64REGS;


#ifndef CC_H2INC
//
// X64 CONTEXT used by Nirvava Inplace Emulator
//
#define X64CONTEXT_DEF              0x00100000L

#define X64CONTEXT_CONTROL          (X64CONTEXT_DEF | 0x1L)
#define X64CONTEXT_INTEGER          (X64CONTEXT_DEF | 0x2L)
#define X64CONTEXT_SEGMENTS         (X64CONTEXT_DEF | 0x4L)
#define X64CONTEXT_FLOATING_POINT   (X64CONTEXT_DEF | 0x8L)

#define X64CONTEXT_FULL             (X64CONTEXT_CONTROL | X64CONTEXT_INTEGER | X64CONTEXT_FLOATING_POINT)
#define X64CONTEXT_EMU              (X64CONTEXT_FULL | X64CONTEXT_SEGMENTS)

//typedef struct __declspec(align(16)) _M128A
typedef struct __declspec(align(16)) _M128A
{
    ULONGLONG Low;
    LONGLONG High;
} M128A, *PM128A;

typedef struct __declspec(align(16)) _X64CONTEXT
{
    // Register parameter home addresses.
    ULONG64 P1Home;
    ULONG64 P2Home;
    ULONG64 P3Home;
    ULONG64 P4Home;
    ULONG64 P5Home;
    ULONG64 P6Home;

    // Control flags.
    ULONG ContextFlags;
    ULONG MxCsr;

    // Segment Registers and processor flags.
    USHORT SegCs;
    USHORT SegDs;
    USHORT SegEs;
    USHORT SegFs;
    USHORT SegGs;
    USHORT SegSs;
    ULONG EFlags;

    // Debug registers
    ULONG64 Dr0;
    ULONG64 Dr1;
    ULONG64 Dr2;
    ULONG64 Dr3;
    ULONG64 Dr6;
    ULONG64 Dr7;

    // Integer registers
    ULONG64 Rax;
    ULONG64 Rcx;
    ULONG64 Rdx;
    ULONG64 Rbx;
    ULONG64 Rsp;
    ULONG64 Rbp;
    ULONG64 Rsi;
    ULONG64 Rdi;
    ULONG64 R8;
    ULONG64 R9;
    ULONG64 R10;
    ULONG64 R11;
    ULONG64 R12;
    ULONG64 R13;
    ULONG64 R14;
    ULONG64 R15;

    // Program counter.
    ULONG64 Rip;

    // Floating point state.
    union
    {
        //M_SAVE_AREA32 FltSave;
        UCHAR FltSave[512];
        struct
        {
            M128A Header[2];
            M128A Legacy[8];
            M128A Xmm0;
            M128A Xmm1;
            M128A Xmm2;
            M128A Xmm3;
            M128A Xmm4;
            M128A Xmm5;
            M128A Xmm6;
            M128A Xmm7;
            M128A Xmm8;
            M128A Xmm9;
            M128A Xmm10;
            M128A Xmm11;
            M128A Xmm12;
            M128A Xmm13;
            M128A Xmm14;
            M128A Xmm15;
        };
    };

    // Vector registers.
    M128A VectorRegister[26];
    ULONG64 VectorControl;

    // Special debug control registers.
    ULONG64 DebugControl;
    ULONG64 LastBranchToRip;
    ULONG64 LastBranchFromRip;
    ULONG64 LastExceptionToRip;
    ULONG64 LastExceptionFromRip;

    // Accessors for popular members
    ULONG64 GetProgramCounter() const { return Rip; }
    ULONG64 GetStackPointer() const { return Rsp; }
    ULONG GetContextFlags() const { return ContextFlags; }
} X64CONTEXT, *PX64CONTEXT;

#endif

#pragma pack()
#pragma warning (pop)

#ifdef __cplusplus
} // namespace
#endif

#endif // X64STATE

