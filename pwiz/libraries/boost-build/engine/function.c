/*
 *  Copyright 2011 Steven Watanabe
 *  Distributed under the Boost Software License, Version 1.0.
 *  (See accompanying file LICENSE_1_0.txt or http://www.boost.org/LICENSE_1_0.txt)
 */

#include "lists.h"
#include "pathsys.h"
#include "mem.h"
#include "constants.h"
#include "jam.h"
#include "frames.h"
#include "function.h"
#include "rules.h"
#include "variable.h"
#include "compile.h"
#include "search.h"
#include "class.h"
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <assert.h>

# ifdef OS_CYGWIN
#  include <sys/cygwin.h>
#  include <windows.h>
# endif

#define INSTR_PUSH_EMPTY                    0
#define INSTR_PUSH_CONSTANT                 1
#define INSTR_PUSH_ARG                      2
#define INSTR_PUSH_VAR                      3
#define INSTR_PUSH_GROUP                    4
#define INSTR_PUSH_RESULT                   5
#define INSTR_PUSH_APPEND                   6
#define INSTR_SWAP                          7

#define INSTR_JUMP_EMPTY                    8
#define INSTR_JUMP_NOT_EMPTY                9

#define INSTR_JUMP                          10
#define INSTR_JUMP_LT                       11
#define INSTR_JUMP_LE                       12
#define INSTR_JUMP_GT                       13
#define INSTR_JUMP_GE                       14
#define INSTR_JUMP_EQ                       15
#define INSTR_JUMP_NE                       16
#define INSTR_JUMP_IN                       17
#define INSTR_JUMP_NOT_IN                   18

#define INSTR_JUMP_NOT_GLOB                 19

#define INSTR_TRY_POP_FRONT                 20

#define INSTR_SET_RESULT                    21
#define INSTR_RETURN                        22
#define INSTR_POP                           23

#define INSTR_PUSH_LOCAL                    24
#define INSTR_POP_LOCAL                     25
#define INSTR_SET                           26
#define INSTR_APPEND                        27
#define INSTR_DEFAULT                       28

#define INSTR_PUSH_LOCAL_GROUP              29
#define INSTR_POP_LOCAL_GROUP               30
#define INSTR_SET_GROUP                     31
#define INSTR_APPEND_GROUP                  32
#define INSTR_DEFAULT_GROUP                 33

#define INSTR_PUSH_ON                       34
#define INSTR_POP_ON                        35
#define INSTR_SET_ON                        36
#define INSTR_APPEND_ON                     37
#define INSTR_DEFAULT_ON                    38

#define INSTR_CALL_RULE                     39

#define INSTR_APPLY_MODIFIERS               40
#define INSTR_APPLY_INDEX                   41
#define INSTR_APPLY_INDEX_MODIFIERS         42
#define INSTR_APPLY_MODIFIERS_GROUP         43
#define INSTR_APPLY_INDEX_GROUP             44
#define INSTR_APPLY_INDEX_MODIFIERS_GROUP   45
#define INSTR_COMBINE_STRINGS               46

#define INSTR_INCLUDE                       47
#define INSTR_RULE                          48
#define INSTR_ACTIONS                       49
#define INSTR_PUSH_MODULE                   50
#define INSTR_POP_MODULE                    51
#define INSTR_CLASS                         52

#define INSTR_APPEND_STRINGS                53
#define INSTR_WRITE_FILE                    54

typedef struct instruction
{
    unsigned int op_code;
    int          arg;
} instruction;

typedef struct _subfunction
{
    OBJECT * name;
    FUNCTION * code;
    int arguments;
    int local;
} SUBFUNCTION;

typedef struct _subaction
{
    OBJECT * name;
    OBJECT * command;
    int      flags;
} SUBACTION;

#define FUNCTION_BUILTIN    0
#define FUNCTION_JAM        1

struct _function
{
    int type;
    int reference_count;
    OBJECT * rulename;
};

typedef struct _builtin_function
{
    int type;
    int reference_count;
    OBJECT * rulename;
    LIST * ( * func )( FRAME *, int flags );
    int flags;
} BUILTIN_FUNCTION;

typedef struct _jam_function
{
    int type;
    int reference_count;
    OBJECT * rulename;
    instruction * code;
    int num_constants;
    OBJECT * * constants;
    int num_subfunctions;
    SUBFUNCTION * functions;
    int num_subactions;
    SUBACTION * actions;
    OBJECT * file;
    int line;
} JAM_FUNCTION;


struct _stack
{
    void * data;
};

static void * stack;

STACK * stack_global()
{
    static STACK result;
    if ( !stack )
    {
        int size = 1 << 21;
        stack = BJAM_MALLOC( size );
        result.data = (char *)stack + size;
    }
    return &result;
}

void stack_push( STACK * s, LIST * l )
{
    *--(*(LIST * * *)&s->data) = l;
}

LIST * stack_pop( STACK * s )
{
    return *(*(LIST * * *)&s->data)++;
}

LIST * stack_top(STACK * s)
{
    return *(LIST * *)s->data;
}

LIST * stack_at( STACK * s, int n )
{
    return *((LIST * *)s->data + n);
}

void stack_set( STACK * s, int n, LIST * value )
{
    *((LIST * *)s->data + n) = value;
}

void * stack_get( STACK * s )
{
    return (LIST * *)s->data;
}

void * stack_allocate( STACK * s, int size )
{
    *(char * *)&s->data -= size;
    return s->data;
}

void stack_deallocate( STACK * s, int size )
{
    *(char * *)&s->data += size;
}

LIST * frame_get_local( FRAME * frame, int idx )
{
    /* The only local variables are the arguments */
    return list_copy( L0, lol_get( frame->args, idx ) );
}

static OBJECT * function_get_constant( JAM_FUNCTION * function, int idx )
{
    return function->constants[ idx ];
}

static LIST * function_get_variable( JAM_FUNCTION * function, FRAME * frame, int idx )
{
    return list_copy( L0, var_get( function->constants[idx] ) );
}

static void function_set_variable( JAM_FUNCTION * function, FRAME * frame, int idx, LIST * value )
{
    var_set( function->constants[idx], value, VAR_SET );
}

static LIST * function_swap_variable( JAM_FUNCTION * function, FRAME * frame, int idx, LIST * value )
{
    return var_swap( function->constants[idx], value );
}

static void function_append_variable( JAM_FUNCTION * function, FRAME * frame, int idx, LIST * value )
{
    var_set( function->constants[idx], value, VAR_APPEND );
}

static void function_default_variable( JAM_FUNCTION * function, FRAME * frame, int idx, LIST * value )
{
    var_set( function->constants[idx], value, VAR_DEFAULT );
}

static void function_set_rule( JAM_FUNCTION * function, FRAME * frame, STACK * s, int idx )
{
    SUBFUNCTION * sub = function->functions + idx;
    argument_list * args = 0;

    if ( sub->arguments )
    {
        int i;
        args = args_new();
        for ( i = sub->arguments; i > 0; --i )
        {
            lol_add( args->data, stack_at( s, i - 1 ) );
        }
       
        for ( i = 0; i < sub->arguments; ++i )
        {
            stack_pop( s );
        }
    }

    new_rule_body( frame->module, sub->name, args, sub->code, !sub->local );
}

static void function_set_actions( JAM_FUNCTION * function, FRAME * frame, STACK * s, int idx )
{
    SUBACTION * sub = function->actions + idx;
    LIST * bindlist = stack_pop( s );

    new_rule_actions( frame->module, sub->name, sub->command, bindlist, sub->flags );
}

/*
 * returns the index if name is "<", ">", "1", "2", ... or "19"
 * otherwise returns -1.
 */

static int get_argument_index( const char * s )
{
    if( s[ 0 ] != '\0')
    {
        if( s[ 1 ] == '\0' )
        {
            switch ( s[ 0 ] )
            {
                case '<': return 0;
                case '>': return 1;

                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return s[ 0 ] - '1';
            }
        }
        else if ( s[ 0 ] == '1' && s[ 2 ] == '\0' )
        {
            switch( s[ 1 ] )
            {
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                return s[ 1 ] - '0' + 10 - 1;
            }
        }
    }
    return -1;
}

static LIST * function_get_named_variable( JAM_FUNCTION * function, FRAME * frame, OBJECT * name )
{
    int idx = get_argument_index( object_str( name ) );
    if( idx != -1 )
    {
        return list_copy( L0, lol_get( frame->args, idx ) );
    }
    else
    {
        return list_copy( L0, var_get( name ) );
    }
}

static void function_set_named_variable( JAM_FUNCTION * function, FRAME * frame, OBJECT * name, LIST * value)
{
    var_set( name, value, VAR_SET );
}

static LIST * function_swap_named_variable( JAM_FUNCTION * function, FRAME * frame, OBJECT * name, LIST * value )
{
    return var_swap( name, value );
}

static void function_append_named_variable( JAM_FUNCTION * function, FRAME * frame, OBJECT * name, LIST * value)
{
    var_set( name, value, VAR_APPEND );
}

static void function_default_named_variable( JAM_FUNCTION * function, FRAME * frame, OBJECT * name, LIST * value )
{
    var_set( name, value, VAR_DEFAULT );
}

static LIST * function_call_rule( JAM_FUNCTION * function, FRAME * frame, STACK * s, int n_args, const char * unexpanded, OBJECT * file, int line )
{
    FRAME   inner[ 1 ];
    int i;
    LIST * first = stack_pop( s );
    LIST * result = L0;
    OBJECT * rulename;

    frame->file = file;
    frame->line = line;
    
    if ( !first )
    {
        backtrace_line( frame );
        printf( "warning: rulename %s expands to empty string\n", unexpanded );
        backtrace( frame );
        return result;
    }

    rulename = object_copy( first->value );

    frame_init( inner );

    inner->prev = frame;
    inner->prev_user = frame->module->user_module ? frame : frame->prev_user;
    inner->module = frame->module;  /* This gets fixed up in evaluate_rule(), below. */

    for( i = 0; i < n_args; ++i )
    {
        lol_add( inner->args, stack_at( s, n_args - i - 1 ) );
    }

    for( i = 0; i < n_args; ++i )
    {
        stack_pop( s );
    }

    if ( inner->args->count == 0 )
    {
        lol_add( inner->args, list_pop_front( first ) );
    }
    else
    {
        LIST * * l = &inner->args->list[0];
        *l = list_append( list_pop_front( first ), *l );
    }

    result = evaluate_rule( rulename, inner );
    frame_free( inner );
    object_free( rulename );
    return result;
}

/* Variable expansion */

typedef struct
{
    int sub1;
    int sub2;
} subscript_t;

typedef struct
{
    PATHNAME f;           /* :GDBSMR -- pieces */
    char     parent;      /* :P -- go to parent directory */
    char     filemods;    /* one of the above applied */
    char     downshift;   /* :L -- downshift result */
    char     upshift;     /* :U -- upshift result */
    char     to_slashes;  /* :T -- convert "\" to "/" */
    char     to_windows;  /* :W -- convert cygwin to native paths */
    PATHPART empty;       /* :E -- default for empties */
    PATHPART join;        /* :J -- join list with char */
} VAR_EDITS;

static LIST * apply_modifiers_impl( LIST * result, string * buf, VAR_EDITS * edits, int n, LIST * iter, LIST * end );
static void get_iters( subscript_t subscript, LIST * * first, LIST * * last, int length );
static void var_edit_file( const char * in, string * out, VAR_EDITS * edits );
static void var_edit_shift( string * out, size_t pos, VAR_EDITS * edits );
static int var_edit_parse( const char * mods, VAR_EDITS * edits, int havezeroed );


/*
 * var_edit_parse() - parse : modifiers into PATHNAME structure
 *
 * The : modifiers in a $(varname:modifier) currently support replacing or
 * omitting elements of a filename, and so they are parsed into a PATHNAME
 * structure (which contains pointers into the original string).
 *
 * Modifiers of the form "X=value" replace the component X with the given value.
 * Modifiers without the "=value" cause everything but the component X to be
 * omitted. X is one of:
 *
 *  G <grist>
 *  D directory name
 *  B base name
 *  S .suffix
 *  M (member)
 *  R root directory - prepended to whole path
 *
 * This routine sets:
 *
 *  f->f_xxx.ptr = 0
 *  f->f_xxx.len = 0
 *      -> leave the original component xxx
 *
 *  f->f_xxx.ptr = string
 *  f->f_xxx.len = strlen( string )
 *      -> replace component xxx with string
 *
 *  f->f_xxx.ptr = ""
 *  f->f_xxx.len = 0
 *      -> omit component xxx
 *
 * var_edit_file() below and path_build() obligingly follow this convention.
 */

static int var_edit_parse( const char * mods, VAR_EDITS * edits, int havezeroed )
{
    while ( *mods )
    {
        PATHPART * fp;

        switch ( *mods++ )
        {
            case 'L': edits->downshift = 1; continue;
            case 'U': edits->upshift = 1; continue;
            case 'P': edits->parent = edits->filemods = 1; continue;
            case 'E': fp = &edits->empty; goto strval;
            case 'J': fp = &edits->join; goto strval;
            case 'G': fp = &edits->f.f_grist; goto fileval;
            case 'R': fp = &edits->f.f_root; goto fileval;
            case 'D': fp = &edits->f.f_dir; goto fileval;
            case 'B': fp = &edits->f.f_base; goto fileval;
            case 'S': fp = &edits->f.f_suffix; goto fileval;
            case 'M': fp = &edits->f.f_member; goto fileval;
            case 'T': edits->to_slashes = 1; continue;
            case 'W': edits->to_windows = 1; continue;
            default:
                break;  /* Should complain, but so what... */
        }

    fileval:
        /* Handle :CHARS, where each char (without a following =) selects a
         * particular file path element. On the first such char, we deselect all
         * others (by setting ptr = "", len = 0) and for each char we select
         * that element (by setting ptr = 0).
         */
        edits->filemods = 1;

        if ( *mods != '=' )
        {
            if ( !havezeroed++ )
            {
                int i;
                for ( i = 0; i < 6; ++i )
                {
                    edits->f.part[ i ].len = 0;
                    edits->f.part[ i ].ptr = "";
                }
            }

            fp->ptr = 0;
            continue;
        }

    strval:
        /* Handle :X=value, or :X */
        if ( *mods != '=' )
        {
            fp->ptr = "";
            fp->len = 0;
        }
        else
        {
            fp->ptr = ++mods;
            fp->len = strlen( mods );
            mods += fp->len;
        }
    }

    return havezeroed;
}

/*
 * var_edit_file() - copy input target name to output, modifying filename.
 */

static void var_edit_file( const char * in, string * out, VAR_EDITS * edits )
{
    if ( edits->filemods )
    {
        PATHNAME pathname;

        /* Parse apart original filename, putting parts into "pathname". */
        path_parse( in, &pathname );

        /* Replace any pathname with edits->f */
        if ( edits->f.f_grist .ptr ) pathname.f_grist  = edits->f.f_grist;
        if ( edits->f.f_root  .ptr ) pathname.f_root   = edits->f.f_root;
        if ( edits->f.f_dir   .ptr ) pathname.f_dir    = edits->f.f_dir;
        if ( edits->f.f_base  .ptr ) pathname.f_base   = edits->f.f_base;
        if ( edits->f.f_suffix.ptr ) pathname.f_suffix = edits->f.f_suffix;
        if ( edits->f.f_member.ptr ) pathname.f_member = edits->f.f_member;

        /* If requested, modify pathname to point to parent. */
        if ( edits->parent )
            path_parent( &pathname );

        /* Put filename back together. */
        path_build( &pathname, out, 0 );
    }
    else
    {
        string_append( out, in );
    }
}

/*
 * var_edit_shift() - do upshift/downshift mods.
 */

static void var_edit_shift( string * out, size_t pos, VAR_EDITS * edits )
{
    if ( edits->upshift || edits->downshift || edits->to_windows || edits->to_slashes )
    {
        /* Handle upshifting, downshifting and slash translation now. */
        char * p;
# ifdef OS_CYGWIN
        if ( edits->to_windows )
        {
            /* FIXME: skip grist */
            char result[ MAX_PATH + 1 ];
            cygwin_conv_to_win32_path( out->value + pos, result );
            assert( strlen( result ) <= MAX_PATH );
            string_truncate( out, pos );
            string_append( out, result );
            edits->to_slashes = 0;
        }
# endif
        for ( p = out->value + pos; *p; ++p)
        {
            if ( edits->upshift )
                *p = toupper( *p );
            else if ( edits->downshift )
                *p = tolower( *p );
            if ( edits->to_slashes && ( *p == '\\' ) )
                *p = '/';
        }
    }
}

/*
 * Reads n LISTs from the top of the STACK and
 * combines them to form VAR_EDITS.
 *
 * returns the number of VAR_EDITS pushed onto
 * the STACK.
 */

static int expand_modifiers( STACK * s, int n )
{
    int i;
    int total = 1;
    LIST * * args = stack_get( s );
    for( i = 0; i < n; ++i)
        total *= list_length( args[i] );

    if ( total != 0 )
    {
        VAR_EDITS * out = stack_allocate( s, total * sizeof(VAR_EDITS) );
        LIST * * iter = stack_allocate( s, n * sizeof(LIST *) );
        for (i = 0; i < n; ++i )
        {
            iter[i] = args[i];
        }
        i = 0;
        {
            int havezeroed;
        loop:
            memset( out, 0, sizeof( *out ) );
            havezeroed = 0;
            for (i = 0; i < n; ++i )
            {
                havezeroed = var_edit_parse( object_str( iter[i]->value ), out, havezeroed );
            }
            ++out;
            while ( --i >= 0 )
            {
                if ( iter[i]->next )
                {
                    iter[i] = iter[i]->next;
                    goto loop;
                }
                else
                {
                    iter[i] = args[i];
                }
            }
        }
        stack_deallocate( s, n * sizeof( LIST * ) );
    }
    return total;
}

static LIST * apply_modifiers( STACK * s, int n )
{
    LIST * value = stack_top( s );
    LIST * result = L0;
    VAR_EDITS * edits = (VAR_EDITS *)( (LIST * *)stack_get( s ) + 1 );
    string buf[1];
    string_new( buf );
    result = apply_modifiers_impl( result, buf, edits, n, value, L0 );
    string_free( buf );
    return result;
}

/*
 * Parse a string of the form "1-2", "-2--1", "2-"
 * and return the two subscripts.
 */

subscript_t parse_subscript( const char * s )
{
    subscript_t result;
    result.sub1 = 0;
    result.sub2 = 0;
    do  /* so we can use "break" */
    {
        /* Allow negative subscripts. */
        if ( !isdigit( *s ) && ( *s != '-' ) )
        {
            result.sub2 = 0;
            break;
        }
        result.sub1 = atoi( s );

        /* Skip over the first symbol, which is either a digit or dash. */
        ++s;
        while ( isdigit( *s ) ) ++s;

        if ( *s == '\0' )
        {
            result.sub2 = result.sub1;
            break;
        }

        if ( *s != '-' )
        {
            result.sub2 = 0;
            break;
        }

        ++s;

        if ( *s == '\0' )
        {
            result.sub2 = -1;
            break;
        }

        if ( !isdigit( *s ) && ( *s != '-' ) )
        {
            result.sub2 = 0;
            break;
        }

        /* First, compute the index of the last element. */
        result.sub2 = atoi( s );
        while ( isdigit( *++s ) );

        if ( *s != '\0' )
            result.sub2 = 0;

    } while ( 0 );
    return result;
}

static LIST * apply_subscript( STACK * s )
{
    LIST * value = stack_top( s );
    LIST * indices = stack_at( s, 1 );
    LIST * result = L0;
    int length = list_length( value );
    string buf[1];
    string_new( buf );
    for ( ; indices; indices = list_next( indices ) )
    {
        LIST * iter = value;
        LIST * end;
        subscript_t subscript = parse_subscript( object_str( indices->value ) );
        get_iters( subscript, &iter, &end, length );
        for ( ; iter != end; iter = list_next( iter ) )
        {
            result = list_new( result, object_copy( iter->value ) );
        }
    }
    string_free( buf );
    return result;
}

/*
 * Reads the LIST from first and applies subscript to it.
 * The results are written to *first and *last.
 */

static void get_iters( subscript_t subscript, LIST * * first, LIST * * last, int length )
{
    int start;
    int size;
    LIST * iter;
    LIST * end;
    {

        if ( subscript.sub1 < 0 )
            start = length + subscript.sub1;
        else if( subscript.sub1 > length )
            start = length;
        else
            start = subscript.sub1 - 1;

        if ( subscript.sub2 < 0 )
            size = length + 1 + subscript.sub2 - start;
        else
            size = subscript.sub2 - start;

        /*
         * HACK: When the first subscript is before the start of the
         * list, it magically becomes the beginning of the list.
         * This is inconsistent, but needed for backwards
         * compatibility.
         */
        if ( start < 0 )
            start = 0;

        /* The "sub2 < 0" test handles the semantic error of sub2 <
            * sub1.
            */
        if ( size < 0 )
            size = 0;

        if ( start + size > length )
            size = length - start;
    }

    iter = *first;
    while ( start-- > 0 )
        iter = list_next( iter );

    end = iter;
    while ( size-- > 0 )
        end = list_next( end );

    *first = iter;
    *last = end;
}

static LIST * apply_modifiers_empty( LIST * result, string * buf, VAR_EDITS * edits, int n)
{
    int i;
    for ( i = 0; i < n; ++i )
    {
        if ( edits[i].empty.ptr )
        {
            /** FIXME: is empty.ptr always null-terminated? */
            var_edit_file( edits[i].empty.ptr, buf, edits + i );
            var_edit_shift( buf, 0, edits + i );
            result = list_new( result, object_new( buf->value ) );
            string_truncate( buf, 0 );
        }
    }
    return result;
}

static LIST * apply_modifiers_non_empty( LIST * result, string * buf, VAR_EDITS * edits, int n, LIST * begin, LIST * end )
{
    int i;
    LIST * iter;
    for ( i = 0; i < n; ++i )
    {
        if ( edits[i].join.ptr )
        {
            var_edit_file( object_str( begin->value ), buf, edits + i );
            var_edit_shift( buf, 0, edits + i );
            for ( iter = list_next( begin ); iter != end; iter = list_next( iter ) )
            {
                size_t size;
                string_append( buf, edits[i].join.ptr );
                size = buf->size;
                var_edit_file( object_str( iter->value ), buf, edits + i );
                var_edit_shift( buf, size, edits + i );
            }
            result = list_new( result, object_new( buf->value ) );
            string_truncate( buf, 0 );
        }
        else
        {
            for ( iter = begin; iter != end; iter = iter->next )
            {
                var_edit_file( object_str( iter->value ), buf, edits + i );
                var_edit_shift( buf, 0, edits + i );
                result = list_new( result, object_new( buf->value ) );
                string_truncate( buf, 0 );
            }
        }
    }
    return result;
}

static LIST * apply_modifiers_impl( LIST * result, string * buf, VAR_EDITS * edits, int n, LIST * iter, LIST * end )
{
    if ( iter != end )
    {
        return apply_modifiers_non_empty( result, buf, edits, n, iter, end );
    }
    else
    {
        return apply_modifiers_empty( result, buf, edits, n );
    }
}

static LIST * apply_subscript_and_modifiers( STACK * s, int n )
{
    LIST * value = stack_top( s );
    LIST * indices = stack_at( s, 1 );
    LIST * result = L0;
    VAR_EDITS * edits = (VAR_EDITS *)((LIST * *)stack_get( s ) + 2);
    int length = list_length( value );
    string buf[1];
    string_new( buf );
    for ( ; indices; indices = list_next( indices ) )
    {
        LIST * iter = value;
        LIST * end;
        subscript_t sub = parse_subscript( object_str( indices->value ) );
        get_iters( sub, &iter, &end, length );
        result = apply_modifiers_impl( result, buf, edits, n, iter, end );
    }
    string_free( buf );
    return result;
}

typedef struct expansion_item
{
    LIST * elem;
    LIST * saved;
    int size;
} expansion_item;

static LIST * expand( expansion_item * elem, int length )
{
    LIST * result = L0;
    string buf[1];
    int size = 0;
    int i;
    assert( length > 0 );
    for ( i = 0; i < length; ++i )
    {
        int max = 0;
        LIST * l;
        if ( !elem[i].elem ) return result;
        for ( l = elem[i].elem; l; l = l->next )
        {
            int len = strlen( object_str( l->value ) );
            if ( len > max ) max = len;
        }
        size += max;
    }
    string_new( buf );
    string_reserve( buf, size );
    i = 0;
    {
    loop:
        for ( ; i < length; ++i )
        {
            elem[i].size = buf->size;
            string_append( buf, object_str( elem[i].elem->value ) );
        }
        result = list_new( result, object_new( buf->value ) );
        while ( --i >= 0 )
        {
            if(elem[i].elem->next)
            {
                elem[i].elem = elem[i].elem->next;
                string_truncate( buf, elem[i].size );
                goto loop;
            }
            else
            {
                elem[i].elem = elem[i].saved;
            }
        }
    }
    string_free( buf );
    return result;
}

static void combine_strings( STACK * s, int n, string * out )
{
    int i;
    LIST * l;
    for ( i = 0; i < n; ++i )
    {
        LIST * values = stack_pop( s );
        if ( values )
        {
            string_append( out, object_str( values->value ) );
            for ( l = list_next( values ); l; l = list_next( l ) )
            {
                string_push_back( out, ' ' );
                string_append( out, object_str( l->value ) );
            }
            list_free( values );
        }
    }
}

struct dynamic_array
{
    int size;
    int capacity;
    void * data;
};

static void dynamic_array_init( struct dynamic_array * array )
{
    array->size = 0;
    array->capacity = 0;
    array->data = 0;
}

static void dynamic_array_free( struct dynamic_array * array )
{
    BJAM_FREE( array->data );
}

static void dynamic_array_push_impl( struct dynamic_array * array, void * value, int unit_size )
{
    if ( array->capacity == 0 )
    {
        array->capacity = 2;
        array->data = BJAM_MALLOC( array->capacity * unit_size );
    }
    else if ( array->capacity == array->size )
    {
        void * new_data;
        array->capacity *= 2;
        new_data = BJAM_MALLOC( array->capacity * unit_size );
        memcpy( new_data, array->data, array->size * unit_size  );
        BJAM_FREE( array->data );
        array->data = new_data;
    }
    memcpy( (char *)array->data + array->size * unit_size, value, unit_size );
    ++array->size;
}

#define dynamic_array_push( array, value ) ( dynamic_array_push_impl( array, &value, sizeof(value) ) )
#define dynamic_array_at( type, array, idx ) (((type *)(array)->data)[idx])

/*
 * struct compiler
 */

struct label_info
{
    int absolute_position;
    struct dynamic_array uses[1];
};

struct stored_rule
{
    OBJECT * name;
    PARSE * parse;
    int arguments;
    int local;
};

typedef struct compiler
{
    struct dynamic_array code[1];
    struct dynamic_array constants[1];
    struct dynamic_array labels[1];
    struct dynamic_array rules[1];
    struct dynamic_array actions[1];
} compiler;

static void compiler_init( compiler * c )
{
    dynamic_array_init( c->code );
    dynamic_array_init( c->constants );
    dynamic_array_init( c->labels );
    dynamic_array_init( c->rules );
    dynamic_array_init( c->actions );
}

static void compiler_free( compiler * c )
{
    int i;
    dynamic_array_free( c->actions );
    dynamic_array_free( c->rules );
    for ( i = 0; i < c->labels->size; ++i )
    {
        dynamic_array_free( dynamic_array_at( struct label_info, c->labels, i ).uses );
    }
    dynamic_array_free( c->labels );
    dynamic_array_free( c->constants );
    dynamic_array_free( c->code );
}

static void compile_emit_instruction( compiler * c, instruction instr )
{
    dynamic_array_push( c->code, instr );
}

static int compile_new_label( compiler * c )
{
    int result = c->labels->size;
    struct label_info info;
    info.absolute_position = -1;
    dynamic_array_init( info.uses );
    dynamic_array_push( c->labels, info );
    return result;
}

static void compile_set_label( compiler * c, int label )
{
    struct label_info * l = &dynamic_array_at( struct label_info, c->labels, label );
    int pos = c->code->size;
    int i;
    assert( l->absolute_position == -1 );
    l->absolute_position = pos;
    for ( i = 0; i < l->uses->size; ++i )
    {
        int id = dynamic_array_at( int, l->uses, i );
        int offset = (int)(pos - id - 1);
        dynamic_array_at( instruction, c->code, id ).arg = offset;
    }
}

static void compile_emit( compiler * c, unsigned int op_code, int arg )
{
    instruction instr;
    instr.op_code = op_code;
    instr.arg = arg;
    compile_emit_instruction( c, instr );
}

static void compile_emit_branch( compiler * c, unsigned int op_code, int label )
{
    struct label_info * l = &dynamic_array_at( struct label_info, c->labels, label );
    int pos = c->code->size;
    instruction instr;
    instr.op_code = op_code;
    if ( l->absolute_position == -1 )
    {
        instr.arg = 0;
        dynamic_array_push( l->uses, pos );
    }
    else
    {
        instr.arg = (int)( l->absolute_position - pos - 1 );
    }
    compile_emit_instruction( c, instr );
}

static int compile_emit_constant( compiler * c, OBJECT * value )
{
    OBJECT * copy = object_copy( value );
    dynamic_array_push( c->constants, copy );
    return c->constants->size - 1;
}

static int compile_emit_rule( compiler * c, OBJECT * name, PARSE * parse, int arguments, int local )
{
    struct stored_rule rule;
    rule.name = object_copy( name );
    rule.parse = parse;
    rule.arguments = arguments;
    rule.local = local;
    dynamic_array_push( c->rules, rule );
    return (int)( c->rules->size - 1 );
}

static int compile_emit_actions( compiler * c, OBJECT * name, OBJECT * command, int flags )
{
    SUBACTION a;
    a.name = object_copy( name );
    a.command = object_copy( command );
    a.flags = flags;
    dynamic_array_push( c->actions, a );
    return (int)( c->actions->size - 1 );
}

static JAM_FUNCTION * compile_to_function( compiler * c )
{
    JAM_FUNCTION * result = BJAM_MALLOC( sizeof(JAM_FUNCTION) );
    int i;
    result->type = FUNCTION_JAM;
    result->reference_count = 1;

    result->rulename = 0;

    result->code = BJAM_MALLOC( c->code->size * sizeof(instruction) );
    memcpy( result->code, c->code->data, c->code->size * sizeof(instruction) );

    result->constants = BJAM_MALLOC( c->constants->size * sizeof(OBJECT *) );
    memcpy( result->constants, c->constants->data, c->constants->size * sizeof(OBJECT *) );
    result->num_constants = c->constants->size;

    result->num_subfunctions = c->rules->size;
    result->functions = BJAM_MALLOC( c->rules->size * sizeof(SUBFUNCTION) );
    for ( i = 0; i < c->rules->size; ++i )
    {
        struct stored_rule * rule = &dynamic_array_at( struct stored_rule, c->rules, i );
        result->functions[i].name = rule->name;
        result->functions[i].code = function_compile( rule->parse );
        result->functions[i].arguments = rule->arguments;
        result->functions[i].local = rule->local;
    }

    result->actions = BJAM_MALLOC( c->actions->size * sizeof(SUBACTION) );
    memcpy( result->actions, c->actions->data, c->actions->size * sizeof(SUBACTION) );
    result->num_subactions = c->actions->size;

    result->file = 0;
    result->line = -1;

    return result;
}

/*
 * Parsing of variable expansions
 */

typedef struct VAR_PARSE_GROUP
{
    struct dynamic_array elems[1];
} VAR_PARSE_GROUP;

#define VAR_PARSE_TYPE_VAR      0
#define VAR_PARSE_TYPE_STRING   1
#define VAR_PARSE_TYPE_FILE     2

typedef struct _var_parse
{
    int type; /* string or variable */
} VAR_PARSE;

typedef struct
{
    int type;
    VAR_PARSE_GROUP * name;
    VAR_PARSE_GROUP * subscript;
    struct dynamic_array modifiers[1];
} VAR_PARSE_VAR;

typedef struct
{
    int type;
    OBJECT * s;
} VAR_PARSE_STRING;

typedef struct
{
    int type;
    struct dynamic_array filename[1];
    struct dynamic_array contents[1];
} VAR_PARSE_FILE;

static void var_parse_free( VAR_PARSE * );

/*
 * VAR_PARSE_GROUP
 */

static VAR_PARSE_GROUP * var_parse_group_new()
{
    VAR_PARSE_GROUP * result = BJAM_MALLOC( sizeof( VAR_PARSE_GROUP ) );
    dynamic_array_init( result->elems );
    return result;
}

static void var_parse_group_free( VAR_PARSE_GROUP * group )
{
    int i;
    for ( i = 0; i < group->elems->size; ++i )
    {
        var_parse_free( dynamic_array_at( VAR_PARSE *, group->elems, i ) );
    }
    dynamic_array_free( group->elems );
    BJAM_FREE( group );
}

static void var_parse_group_add( VAR_PARSE_GROUP * group, VAR_PARSE * elem )
{
    dynamic_array_push( group->elems, elem );
}

static void var_parse_group_maybe_add_constant( VAR_PARSE_GROUP * group, const char * start, const char * end )
{
    if ( start != end )
    {
        string buf[1];
        VAR_PARSE_STRING * value = (VAR_PARSE_STRING *)BJAM_MALLOC( sizeof(VAR_PARSE_STRING) );
        value->type = VAR_PARSE_TYPE_STRING;
        string_new( buf );
        string_append_range( buf, start, end );
        value->s = object_new( buf->value );
        string_free( buf );
        var_parse_group_add( group, (VAR_PARSE *)value );
    }
}

/*
 * VAR_PARSE_VAR
 */

static VAR_PARSE_VAR * var_parse_var_new()
{
    VAR_PARSE_VAR * result = BJAM_MALLOC( sizeof( VAR_PARSE_VAR ) );
    result->type = VAR_PARSE_TYPE_VAR;
    result->name = var_parse_group_new();
    result->subscript = 0;
    dynamic_array_init( result->modifiers );
    return result;
}

static void var_parse_var_free( VAR_PARSE_VAR * var )
{
    int i;
    var_parse_group_free( var->name );
    if ( var->subscript )
        var_parse_group_free( var->subscript );
    for( i = 0; i < var->modifiers->size; ++i )
        var_parse_group_free( dynamic_array_at( VAR_PARSE_GROUP *, var->modifiers, i ) );
    dynamic_array_free( var->modifiers );
    BJAM_FREE( var );
}

static VAR_PARSE_GROUP * var_parse_var_new_modifier( VAR_PARSE_VAR * var )
{
    VAR_PARSE_GROUP * result = var_parse_group_new();
    dynamic_array_push( var->modifiers, result );
    return result;
}

/*
 * VAR_PARSE_STRING
 */

static void var_parse_string_free( VAR_PARSE_STRING * string )
{
    object_free( string->s );
    BJAM_FREE( string );
}

/*
 * VAR_PARSE_FILE
 */

static VAR_PARSE_FILE * var_parse_file_new( void )
{
    VAR_PARSE_FILE * result = (VAR_PARSE_FILE *)BJAM_MALLOC( sizeof( VAR_PARSE_FILE ) );
    result->type = VAR_PARSE_TYPE_FILE;
    dynamic_array_init( result->filename );
    dynamic_array_init( result->contents );
    return result;
}

static void var_parse_file_free( VAR_PARSE_FILE * file )
{
    int i;
    for( i = 0; i < file->filename->size; ++i )
        var_parse_group_free( dynamic_array_at( VAR_PARSE_GROUP *, file->filename, i ) );
    dynamic_array_free( file->filename );
    for( i = 0; i < file->contents->size; ++i )
        var_parse_group_free( dynamic_array_at( VAR_PARSE_GROUP *, file->contents, i ) );
    dynamic_array_free( file->contents );
    BJAM_FREE( file );
}

/*
 * VAR_PARSE
 */

static void var_parse_free( VAR_PARSE * parse )
{
    if ( parse->type == VAR_PARSE_TYPE_VAR )
    {
        var_parse_var_free( (VAR_PARSE_VAR *)parse );
    }
    else if ( parse->type == VAR_PARSE_TYPE_STRING )
    {
        var_parse_string_free( (VAR_PARSE_STRING *)parse );
    }
    else if ( parse->type == VAR_PARSE_TYPE_FILE )
    {
        var_parse_file_free( (VAR_PARSE_FILE *)parse );
    }
    else
    {
        assert(!"Invalid type");
    }
}

/*
 * Compile VAR_PARSE
 */

static void var_parse_group_compile( const VAR_PARSE_GROUP * parse, compiler * c );

static void var_parse_var_compile( const VAR_PARSE_VAR * parse, compiler * c )
{
    int expand_name = 0;
    /* If there are modifiers, emit them in reverse order. */
    if ( parse->modifiers->size > 0 )
    {
        int i;
        for ( i = 0; i < parse->modifiers->size; ++i )
        {
            var_parse_group_compile( dynamic_array_at( VAR_PARSE_GROUP *, parse->modifiers, parse->modifiers->size - i - 1 ), c );
        }
    }

    /* If there's a subscript, emit it. */
    if ( parse->subscript )
    {
        var_parse_group_compile( parse->subscript, c );
    }

    /* If the variable name is empty, look it up. */
    if ( parse->name->elems->size == 0 )
    {
        compile_emit( c, INSTR_PUSH_VAR, compile_emit_constant( c, constant_empty ) );
    }
    /* If the variable name doesn't need to be expanded, look it up. */
    else if ( parse->name->elems->size == 1 &&
        dynamic_array_at( VAR_PARSE *, parse->name->elems, 0 )->type == VAR_PARSE_TYPE_STRING )
    {
        OBJECT * name = ( (VAR_PARSE_STRING *)dynamic_array_at( VAR_PARSE *, parse->name->elems, 0 ) )->s;
        int idx = get_argument_index( object_str( name ) );
        if ( idx != -1 )
        {
            compile_emit( c, INSTR_PUSH_ARG, idx );
        }
        else
        {
            compile_emit( c, INSTR_PUSH_VAR, compile_emit_constant( c, name ) );
        }
    }
    /* Otherwise, push the var names and use the group instruction. */
    else
    {
        var_parse_group_compile( parse->name, c );
        expand_name = 1;
    }

    /** Select the instruction for expanding the variable. */
    if ( !parse->modifiers->size && !parse->subscript && !expand_name )
    {
        /* Nothing to do */
    }
    else if ( !parse->modifiers->size && !parse->subscript && expand_name )
    {
        compile_emit( c, INSTR_PUSH_GROUP, 0 );
    }
    else if ( !parse->modifiers->size && parse->subscript && !expand_name )
    {
        compile_emit( c, INSTR_APPLY_INDEX, 0 );
    }
    else if ( !parse->modifiers->size && parse->subscript && expand_name )
    {
        compile_emit( c, INSTR_APPLY_INDEX_GROUP, 0 );
    }
    if ( parse->modifiers->size && !parse->subscript && !expand_name )
    {
        compile_emit( c, INSTR_APPLY_MODIFIERS, parse->modifiers->size );
    }
    else if ( parse->modifiers->size && !parse->subscript && expand_name )
    {
        compile_emit( c, INSTR_APPLY_MODIFIERS_GROUP, parse->modifiers->size );
    }
    else if ( parse->modifiers->size && parse->subscript && !expand_name )
    {
        compile_emit( c, INSTR_APPLY_INDEX_MODIFIERS, parse->modifiers->size );
    }
    else if ( parse->modifiers->size && parse->subscript && expand_name )
    {
        compile_emit( c, INSTR_APPLY_INDEX_MODIFIERS_GROUP, parse->modifiers->size );
    }
}

static void var_parse_string_compile( const VAR_PARSE_STRING * parse, compiler * c )
{
    compile_emit( c, INSTR_PUSH_CONSTANT, compile_emit_constant( c, parse->s ) );
}

static void var_parse_file_compile( const VAR_PARSE_FILE * parse, compiler * c )
{
    int i;
    for ( i = 0; i < parse->filename->size; ++i )
    {
        var_parse_group_compile( dynamic_array_at( VAR_PARSE_GROUP *, parse->filename, parse->filename->size - i - 1 ), c );
    }
    compile_emit( c, INSTR_APPEND_STRINGS, parse->filename->size );
    for ( i = 0; i < parse->contents->size; ++i )
    {
        var_parse_group_compile( dynamic_array_at( VAR_PARSE_GROUP *, parse->contents, parse->contents->size - i - 1 ), c );
    }
    compile_emit( c, INSTR_WRITE_FILE, parse->contents->size );
}

static void var_parse_compile( const VAR_PARSE * parse, compiler * c )
{
    if( parse->type == VAR_PARSE_TYPE_VAR )
    {
        var_parse_var_compile( (const VAR_PARSE_VAR *)parse, c );
    }
    else if( parse->type == VAR_PARSE_TYPE_STRING )
    {
        var_parse_string_compile( (const VAR_PARSE_STRING *)parse, c );
    }
    else if( parse->type == VAR_PARSE_TYPE_FILE )
    {
        var_parse_file_compile( (const VAR_PARSE_FILE *)parse, c );
    }
    else
    {
        assert( !"Unknown var parse type." );
    }
}

static void var_parse_group_compile( const VAR_PARSE_GROUP * parse, compiler * c )
{
    /* Emit the elements in reverse order. */
    int i;
    for( i = 0; i < parse->elems->size; ++i)
    {
        var_parse_compile( dynamic_array_at( VAR_PARSE *, parse->elems, parse->elems->size - i - 1 ), c );
    }
    /* If there're no elements, emit an empty string. */
    if ( parse->elems->size == 0 )
    {
        compile_emit( c, INSTR_PUSH_CONSTANT, compile_emit_constant( c, constant_empty ) );
    }
    /* If there's more than one element, combine them. */
    if ( parse->elems->size > 1 )
    {
        compile_emit( c, INSTR_COMBINE_STRINGS, parse->elems->size );
    }
}

/*
 * Parse VAR_PARSE_VAR
 */

static VAR_PARSE * parse_at_file( const char * start, const char * mid, const char * end );
static VAR_PARSE * parse_variable( const char * * string );
static int try_parse_variable( const char * * s_, const char * * string, VAR_PARSE_GROUP * out);
static void balance_parentheses( const char * * s_, const char * * string, VAR_PARSE_GROUP * out);

/*
 * Parses a string that can contain variables to expand.
 */

static VAR_PARSE_GROUP * parse_expansion( const char * * string )
{
    VAR_PARSE_GROUP * result = var_parse_group_new();
    const char * s = *string;
    for (;;)
    {
        if(try_parse_variable( &s, string, result )) {}
        else if(s[0] == '\0')
        {
            var_parse_group_maybe_add_constant( result, *string, s );
            return result;
        }
        else
        {
            ++s;
        }
    }
}

/*
 * Checks whether the string a *s_ starts with
 * a variable expansion "$(".  *string should point
 * to the first unemitted character before *s.
 * If *s_ starts with variable expansion, appends
 * elements to out up to the closing ")", and
 * adjusts *s_ and *string to point to next character.
 * Returns 1 if s_ starts with a variable, 0 otherwise.
 */

static int try_parse_variable( const char * * s_, const char * * string, VAR_PARSE_GROUP * out)
{
    const char * s = *s_;
    if(s[0] == '$' && s[1] == '(')
    {
        var_parse_group_maybe_add_constant( out, *string, s );
        s += 2;
        var_parse_group_add( out, parse_variable( &s ) );
        *string = s;
        *s_ = s;
        return 1;
    }
    else if(s[0] == '@' && s[1] == '(')
    {
        int depth = 1;
        const char * ine;
        const char * split = 0;
        var_parse_group_maybe_add_constant( out, *string, s );
        s += 2;
        ine = s;

        /* Scan the content of the response file @() section. */
        while ( *ine && ( depth > 0 ) )
        {
            switch ( *ine )
            {
            case '(': ++depth; break;
            case ')': --depth; break;
            case ':':
                if ( ( depth == 1 ) && ( ine[ 1 ] == 'E' ) && ( ine[ 2 ] == '=' ) )
                    split = ine;
                break;
            }
            ++ine;
        }
        
        if ( !split || depth != 0 )
        {
            return 0;
        }
        
        var_parse_group_add( out, parse_at_file( s, split, ine - 1 ) );
        *string = ine;
        *s_ = ine;

        return 1;
    }
    else
    {
        return 0;
    }
}

static const char * current_file = "";
static int current_line;

static void parse_error( const char * message )
{
    printf( "%s:%d: %s\n", current_file, current_line, message );
    exit(1);
}

/*
 * Parses a single variable up to the closing ")" and
 * adjusts *string to point to the next character. *string
 * should point to the character immediately after
 * the initial "$("
 */

static VAR_PARSE * parse_variable( const char * * string )
{
    VAR_PARSE_VAR * result = var_parse_var_new();
    VAR_PARSE_GROUP * name = result->name;
    const char * s = *string;
    for ( ; ; )
    {
        if ( try_parse_variable( &s, string, name ) ) {}
        else if ( s[0] == ':' ) 
        {
            VAR_PARSE_GROUP * mod;
            var_parse_group_maybe_add_constant( name, *string, s );
            ++s;
            *string = s;
            mod = var_parse_var_new_modifier( result );
            for ( ; ; )
            {
                if ( try_parse_variable( &s, string, mod ) ) {}
                else if ( s[0] == ')' )
                {
                    var_parse_group_maybe_add_constant( mod, *string, s );
                    ++s;
                    *string = s;
                    return (VAR_PARSE *)result;
                }
                else if ( s[0] == '(' )
                {
                    ++s;
                    balance_parentheses( &s, string, mod );
                }
                else if ( s[0] == ':' )
                {
                    var_parse_group_maybe_add_constant( mod, *string, s );
                    ++s;
                    *string = s;
                    mod = var_parse_var_new_modifier( result );
                }
                else if ( s[0] == '[' )
                {
                    parse_error("unexpected subscript");
                }
                else if ( s[0] == '\0' )
                {
                    parse_error( "unbalanced parentheses" );
                }
                else
                {
                    ++s;
                }
            }
        }
        else if ( s[0] == '[' )
        {
            VAR_PARSE_GROUP * subscript = var_parse_group_new();
            result->subscript = subscript;
            var_parse_group_maybe_add_constant( name, *string, s );
            ++s;
            *string = s;
            for ( ; ; )
            {
                if ( try_parse_variable( &s, string, subscript ) ) {}
                else if ( s[0] == ']' )
                {
                    var_parse_group_maybe_add_constant( subscript, *string, s );
                    ++s;
                    *string = s;
                    if ( s[0] == '\0' )
                    {
                        parse_error( "unbalanced parentheses" );
                    }
                    else if ( s[0] == ')' || s[0] == ':' )
                    {
                        break;
                    }
                    else
                    {
                        parse_error( "unexpected text following []" );
                    }
                }
                else if ( isdigit( s[0] ) || s[0] == '-' )
                {
                    ++s;
                }
                else
                {
                    parse_error( "malformed subscript" );
                }
            }
        }
        else if ( s[0] == ')' )
        {
            var_parse_group_maybe_add_constant( name, *string, s );
            ++s;
            *string = s;
            return (VAR_PARSE *)result;
        }
        else if ( s[0] == '(' )
        {
            ++s;
            balance_parentheses( &s, string, name );
        }
        else if ( s[0] == '\0' ) 
        {
            parse_error( "unbalanced parentheses" );
        }
        else
        {
            ++s;
        }
    }
}

static void parse_var_string( const char * first, const char * last, struct dynamic_array * out )
{
    const char * saved = first;
    string buf[1];
    int state = isspace( *first ) != 0;
    string_new( buf );
    for ( ; ; ++first )
    {
        if ( first == last || ( isspace( *first ) != 0 ) != state )
        {
            VAR_PARSE_GROUP * group;
            const char * s = buf->value;
            string_append_range( buf, saved, first );
            saved = first;
            group = parse_expansion( &s );
            string_truncate( buf, 0 );
            dynamic_array_push( out, group );
            state = !state;
        }
        if ( first == last ) break;
    }
    string_free( buf );
}

/*
 * start should point to the character immediately following the
 * opening "@(", mid should point to the ":E=", and end should
 * point to the closing ")".
 */

static VAR_PARSE * parse_at_file( const char * start, const char * mid, const char * end )
{
    VAR_PARSE_FILE * result = var_parse_file_new();
    parse_var_string( start, mid, result->filename );
    parse_var_string( mid + 3, end, result->contents );
    return (VAR_PARSE *)result;
}

/*
 * Given that *s_ points to the character after a "(",
 * parses up to the matching ")".  *string should
 * point to the first unemitted character before *s_.
 *
 * When the function returns, *s_ will point to the character
 * after the ")", and *string will point to the first
 * unemitted character before *s_.  The range from *string
 * to *s_ does not contain any variables that need to be
 * expanded.
 */

void balance_parentheses( const char * * s_, const char * * string, VAR_PARSE_GROUP * out)
{
    int depth = 1;
    const char * s = *s_;
    for ( ; ; )
    {
        if ( try_parse_variable( &s, string, out ) ) { }
        else if(s[0] == ':' || s[0] == '[' || s[0] == '\0') 
        {
            parse_error( "unbalanced parentheses" );
        }
        else if(s[0] == ')')
        {
            ++s;
            if(--depth == 0) break;
        }
        else if(s[0] == '(')
        {
            ++depth;
            ++s;
        }
        else
        {
            ++s;
        }
    }
    *s_ = s;
}

/*
 * Main compile
 */

#define RESULT_STACK 0
#define RESULT_RETURN 1
#define RESULT_NONE 2

static void compile_parse( PARSE * parse, compiler * c, int result_location );

static void compile_condition( PARSE * parse, compiler * c, int branch_true, int label )
{
    assert( parse->type == PARSE_EVAL );
    switch ( parse->num )
    {
    case EXPR_EXISTS:
        {
            compile_parse( parse->left, c, RESULT_STACK );
            if ( branch_true )
                compile_emit_branch( c, INSTR_JUMP_NOT_EMPTY, label );
            else
                compile_emit_branch( c, INSTR_JUMP_EMPTY, label );
            break;
        }
    case EXPR_EQUALS:
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            if ( branch_true )
                compile_emit_branch( c, INSTR_JUMP_EQ, label );
            else
                compile_emit_branch( c, INSTR_JUMP_NE, label );
            break;
        }
    case EXPR_NOTEQ:
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            if ( branch_true )
                compile_emit_branch( c, INSTR_JUMP_NE, label );
            else
                compile_emit_branch( c, INSTR_JUMP_EQ, label );
            break;
        }
    case EXPR_LESS:
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            if ( branch_true )
                compile_emit_branch( c, INSTR_JUMP_LT, label );
            else
                compile_emit_branch( c, INSTR_JUMP_GE, label );
            break;
        }
    case EXPR_LESSEQ:
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            if ( branch_true )
                compile_emit_branch( c, INSTR_JUMP_LE, label );
            else
                compile_emit_branch( c, INSTR_JUMP_GT, label );
            break;
        }
    case EXPR_MORE:
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            if ( branch_true )
                compile_emit_branch( c, INSTR_JUMP_GT, label );
            else
                compile_emit_branch( c, INSTR_JUMP_LE, label );
            break;
        }
    case EXPR_MOREEQ:
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            if ( branch_true )
                compile_emit_branch( c, INSTR_JUMP_GE, label );
            else
                compile_emit_branch( c, INSTR_JUMP_LT, label );
            break;
        }
    case EXPR_IN:
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            if ( branch_true )
                compile_emit_branch( c, INSTR_JUMP_IN, label );
            else
                compile_emit_branch( c, INSTR_JUMP_NOT_IN, label );
            break;
        }
    case EXPR_AND:
        {
            if ( branch_true )
            {
                int f = compile_new_label( c );
                compile_condition( parse->left, c, 0, f );
                compile_condition( parse->right, c, 1, label );
                compile_set_label( c, f );
            }
            else
            {
                compile_condition( parse->left, c, 0, label );
                compile_condition( parse->right, c, 0, label );
            }
            break;
        }
    case EXPR_OR:
        {
            if ( branch_true )
            {
                compile_condition( parse->left, c, 1, label );
                compile_condition( parse->right, c, 1, label );
            }
            else
            {
                int t = compile_new_label( c );
                compile_condition( parse->left, c, 1, t );
                compile_condition( parse->right, c, 0, label );
                compile_set_label( c, t );
            }
            break;
        }
    case EXPR_NOT:
        {
            compile_condition( parse->left, c, !branch_true, label );
            break;
        }
    }
}

static void adjust_result( compiler * c, int actual_location, int desired_location )
{
    if ( actual_location == desired_location )
        ;
    else if ( actual_location == RESULT_STACK && desired_location == RESULT_RETURN )
        compile_emit( c, INSTR_SET_RESULT, 0 );
    else if( actual_location == RESULT_STACK && desired_location == RESULT_NONE )
        compile_emit( c, INSTR_POP, 0 );
    else if( actual_location == RESULT_RETURN && desired_location == RESULT_STACK )
        compile_emit( c, INSTR_PUSH_RESULT, 0 );
    else if ( actual_location == RESULT_RETURN && desired_location == RESULT_NONE )
        ;
    else if ( actual_location == RESULT_NONE && desired_location == RESULT_STACK )
        compile_emit( c, INSTR_PUSH_EMPTY, 0 );
    else if ( actual_location == RESULT_NONE && desired_location == RESULT_RETURN )
    {
        compile_emit( c, INSTR_PUSH_EMPTY, 0 );
        compile_emit( c, INSTR_SET_RESULT, 0 );
    }
    else
    {
        assert( !"invalid result location" );
    }
}

static const char * parse_type( PARSE * parse )
{
    switch ( parse->type )
    {
        case PARSE_APPEND: return "append";
        case PARSE_EVAL: return "eval";
        case PARSE_RULES: return "rules";
        default: return "unknown";
    }
}

static void compile_parse( PARSE * parse, compiler * c, int result_location )
{
    if ( parse->type == PARSE_APPEND )
    {
        /*
         * append is associative, so flip the parse tree of chained
         * appends around to keep the stack from getting too deep.
         */
        compile_parse( parse->right, c, RESULT_STACK );
        while ( parse->left->type == PARSE_APPEND )
        {
            compile_parse( parse->left->right, c, RESULT_STACK );
            compile_emit( c, INSTR_PUSH_APPEND, 0 );
            parse = parse->left;
        }
        compile_parse( parse->left, c, RESULT_STACK );
        compile_emit( c, INSTR_PUSH_APPEND, 0 );
        adjust_result( c, RESULT_STACK, result_location );
    }
    else if ( parse->type == PARSE_EVAL )
    {
        /* FIXME: This is only needed because of the bizarre parsing of conditions. */
        if ( parse->num == EXPR_EXISTS )
        {
            compile_parse( parse->left, c, result_location );
        }
        else
        {
            printf( "%s:%d: Conditional used as list (check operator precedence).\n", object_str(parse->file), parse->line, parse->num );
            exit( 1 );
        }
    }
    else if ( parse->type == PARSE_FOREACH )
    {
        int var = compile_emit_constant( c, parse->string );
        int top = compile_new_label( c );
        int end = compile_new_label( c );

        /*
         * Evaluate the list.
         */
        compile_parse( parse->left, c, RESULT_STACK );

        /* Localize the loop variable */
        if ( parse->num )
        {
            compile_emit( c, INSTR_PUSH_EMPTY, 0 );
            compile_emit( c, INSTR_PUSH_LOCAL, var );
            compile_emit( c, INSTR_SWAP, 1 );
        }

        compile_set_label( c, top );
        compile_emit_branch( c, INSTR_TRY_POP_FRONT, end );
        compile_emit( c, INSTR_SET, var );
        compile_emit( c, INSTR_POP, 0 );

        /* Run the loop body */
        compile_parse( parse->right, c, RESULT_NONE );

        compile_emit_branch( c, INSTR_JUMP, top );
        compile_set_label( c, end );

        if ( parse->num )
        {
            compile_emit( c, INSTR_POP_LOCAL, var );
        }

        adjust_result( c, RESULT_NONE, result_location);
    }
    else if( parse->type == PARSE_IF )
    {
        int nested_result = result_location == RESULT_NONE? RESULT_NONE : RESULT_RETURN;
        int f = compile_new_label( c );
        /* Emit the condition */
        compile_condition( parse->left, c, 0, f );
        /* Emit the if block */
        compile_parse( parse->right, c, nested_result );
        if ( parse->third->type != PARSE_NULL )
        {
            /* Emit the else block */
            int end = compile_new_label( c );
            compile_emit_branch( c, INSTR_JUMP, end );
            compile_set_label( c, f );
            compile_parse( parse->third, c, nested_result );
            compile_set_label( c, end );
        }
        else
        {
            compile_set_label( c, f );
        }

        adjust_result( c, nested_result, result_location);
    }
    else if( parse->type == PARSE_WHILE )
    {
        int nested_result = result_location == RESULT_NONE? RESULT_NONE : RESULT_RETURN;
        int test = compile_new_label( c );
        int top = compile_new_label( c );
        /* Jump to the loop test */
        compile_emit_branch( c, INSTR_JUMP, test );
        compile_set_label( c, top );
        /* Emit the loop body */
        compile_parse( parse->right, c, nested_result );
        /* Emit the condition */
        compile_set_label( c, test );
        compile_condition( parse->left, c, 1, top );

        adjust_result( c, nested_result, result_location );
    }
    else if ( parse->type == PARSE_INCLUDE )
    {
        compile_parse( parse->left, c, RESULT_STACK );
        compile_emit( c, INSTR_INCLUDE, 0 );
        adjust_result( c, RESULT_NONE, result_location );
    }
    else if ( parse->type == PARSE_MODULE )
    {
        int nested_result = result_location == RESULT_NONE? RESULT_NONE : RESULT_RETURN;
        compile_parse( parse->left, c, RESULT_STACK );
        compile_emit( c, INSTR_PUSH_MODULE, 0 );
        compile_parse( parse->right, c, nested_result );
        compile_emit( c, INSTR_POP_MODULE, 0 );
        adjust_result( c, nested_result, result_location );
    }
    else if ( parse->type == PARSE_CLASS )
    {
        /* Evaluate the class name. */
        compile_parse( parse->left->right, c, RESULT_STACK );
        /* Evaluate the base classes. */
        if ( parse->left->left )
        {
            compile_parse( parse->left->left->right, c, RESULT_STACK );
        }
        else
        {
            compile_emit( c, INSTR_PUSH_EMPTY, 0 );
        }
        compile_emit( c, INSTR_CLASS, 0 );
        compile_parse( parse->right, c, RESULT_NONE );
        compile_emit( c, INSTR_POP_MODULE, 0 );

        adjust_result( c, RESULT_NONE, result_location );
    }
    else if ( parse->type == PARSE_LIST )
    {
        OBJECT * o = parse->string;
        const char * s = object_str( o );
        VAR_PARSE_GROUP * group;
        current_file = object_str( parse->file );
        current_line = parse->line;
        group = parse_expansion( &s );
        var_parse_group_compile( group, c );
        var_parse_group_free( group );
        adjust_result( c, RESULT_STACK, result_location );
    }
    else if ( parse->type == PARSE_LOCAL )
    {
        int nested_result = result_location == RESULT_NONE? RESULT_NONE : RESULT_RETURN;
        /*
         * This should be left recursive group of compile_appends
         */
        PARSE * vars = parse->left;

        /* Special case an empty list of vars */
        if ( vars->type == PARSE_NULL )
        {
            compile_parse( parse->right, c, RESULT_NONE );
            compile_parse( parse->third, c, result_location );
            nested_result = result_location;
        }
        /*
         * Check whether there is exactly one variable
         * with a constant name
         */
        else if ( vars->left->type == PARSE_NULL &&
            vars->right->type == PARSE_LIST )
        {
            const char * s = object_str( vars->right->string );
            VAR_PARSE_GROUP * group;
            current_file = object_str( parse->file );
            current_line = parse->line;
            group = parse_expansion( &s );
            if ( group->elems->size == 1 &&
                dynamic_array_at( VAR_PARSE *, group->elems, 0 )->type == VAR_PARSE_TYPE_STRING )
            {
                int name = compile_emit_constant( c, ( (VAR_PARSE_STRING *)dynamic_array_at( VAR_PARSE *, group->elems, 0 ) )->s );
                var_parse_group_free( group );
                compile_parse( parse->right, c, RESULT_STACK );
                compile_emit( c, INSTR_PUSH_LOCAL, name );
                compile_parse( parse->third, c, nested_result );
                compile_emit( c, INSTR_POP_LOCAL, name );
            }
            else
            {
                var_parse_group_compile( group, c );
                var_parse_group_free( group );
                compile_parse( parse->right, c, RESULT_STACK );
                compile_emit( c, INSTR_PUSH_LOCAL_GROUP, 0 );
                compile_parse( parse->third, c, nested_result );
                compile_emit( c, INSTR_POP_LOCAL_GROUP, 0 );
            }
        }
        else
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            compile_emit( c, INSTR_PUSH_LOCAL_GROUP, 0 );
            compile_parse( parse->third, c, nested_result );
            compile_emit( c, INSTR_POP_LOCAL_GROUP, 0 );
        }
        adjust_result( c, nested_result, result_location );
    }
    else if ( parse->type == PARSE_ON )
    {
        int end = compile_new_label( c );
        compile_parse( parse->left, c, RESULT_STACK );
        compile_emit_branch( c, INSTR_PUSH_ON, end );
        compile_parse( parse->right, c, RESULT_STACK );
        compile_emit( c, INSTR_POP_ON, 0 );
        compile_set_label( c, end );
        adjust_result( c, RESULT_STACK, result_location );
    }
    else if ( parse->type == PARSE_RULE )
    {
        PARSE * p;
        int n = 0;
        VAR_PARSE_GROUP * group;
        const char * s = object_str( parse->string );

        if ( parse->left->left == NULL && parse->left->right->type == PARSE_NULL )
            ;
        else
            for ( p = parse->left; p; p = p->left )
            {
                compile_parse( p->right, c, RESULT_STACK );
                ++n;
            }
            
        current_file = object_str( parse->file );
        current_line = parse->line;
        group = parse_expansion( &s );
        var_parse_group_compile( group, c );
        var_parse_group_free( group );
        compile_emit( c, INSTR_CALL_RULE, n );
        compile_emit( c, compile_emit_constant( c, parse->string ), parse->line );
        adjust_result( c, RESULT_STACK, result_location );
    }
    else if ( parse->type == PARSE_RULES )
    {
        do compile_parse( parse->left, c, RESULT_NONE );
        while ( ( parse = parse->right )->type == PARSE_RULES );
        compile_parse( parse, c, result_location );
    }
    else if ( parse->type == PARSE_SET )
    {
        PARSE * vars = parse->left;
        unsigned int op_code;
        unsigned int op_code_group;

        switch ( parse->num )
        {
        case ASSIGN_SET: default: op_code = INSTR_SET; op_code_group = INSTR_SET_GROUP; break;
        case ASSIGN_APPEND: op_code = INSTR_APPEND; op_code_group = INSTR_APPEND_GROUP; break;
        case ASSIGN_DEFAULT: op_code = INSTR_DEFAULT; op_code_group = INSTR_DEFAULT_GROUP; break;
        }

        /*
         * Check whether there is exactly one variable
         * with a constant name
         */
        if ( vars->type == PARSE_LIST )
        {
            const char * s = object_str( vars->string );
            VAR_PARSE_GROUP * group;
            current_file = object_str( parse->file );
            current_line = parse->line;
            group = parse_expansion( &s );
            if ( group->elems->size == 1 &&
                dynamic_array_at( VAR_PARSE *, group->elems, 0 )->type == VAR_PARSE_TYPE_STRING )
            {
                int name = compile_emit_constant( c, ( (VAR_PARSE_STRING *)dynamic_array_at( VAR_PARSE *, group->elems, 0 ) )->s );
                var_parse_group_free( group );
                compile_parse( parse->right, c, RESULT_STACK );
                compile_emit( c, op_code, name );
            }
            else
            {
                var_parse_group_compile( group, c );
                var_parse_group_free( group );
                compile_parse( parse->right, c, RESULT_STACK );
                compile_emit( c, op_code_group, 0 );
            }
        }
        else
        {
            compile_parse( parse->left, c, RESULT_STACK );
            compile_parse( parse->right, c, RESULT_STACK );
            compile_emit( c, op_code_group, 0 );
        }
        adjust_result( c, RESULT_STACK, result_location );
    }
    else if ( parse->type == PARSE_SETCOMP )
    {
        int n_args = 0;
        int rule_id;
        if ( parse->right )
        {
            PARSE * p;
            for ( p = parse->right; p; p = p->left )
            {
                compile_parse( p->right, c, RESULT_STACK );
                ++n_args;
            }
        }

        rule_id = compile_emit_rule( c, parse->string, parse->left, n_args, parse->num );

        compile_emit( c, INSTR_RULE, rule_id );
        adjust_result( c, RESULT_NONE, result_location );
    }
    else if ( parse->type == PARSE_SETEXEC )
    {
        int actions_id = compile_emit_actions( c, parse->string, parse->string1, parse->num );

        compile_parse( parse->left, c, RESULT_STACK );

        compile_emit( c, INSTR_ACTIONS, actions_id );
        adjust_result( c, RESULT_NONE, result_location );
    }
    else if ( parse->type == PARSE_SETTINGS )
    {
        compile_parse( parse->left, c, RESULT_STACK );
        compile_parse( parse->third, c, RESULT_STACK );
        compile_parse( parse->right, c, RESULT_STACK );

        switch ( parse->num )
        {
            case ASSIGN_SET: default: compile_emit( c, INSTR_SET_ON, 0 ); break;
            case ASSIGN_APPEND: compile_emit( c, INSTR_APPEND_ON, 0 ); break;
            case ASSIGN_DEFAULT: compile_emit( c, INSTR_DEFAULT_ON, 0 ); break;
        }

        adjust_result( c, RESULT_STACK, result_location );
    }
    else if ( parse->type  == PARSE_SWITCH )
    {
        int switch_end = compile_new_label( c );
        compile_parse( parse->left, c, RESULT_STACK );

        for ( parse = parse->right; parse; parse = parse->right )
        {
            int id = compile_emit_constant( c, parse->left->string );
            int next_case = compile_new_label( c );
            compile_emit( c, INSTR_PUSH_CONSTANT, id );
            compile_emit_branch( c, INSTR_JUMP_NOT_GLOB, next_case );
            compile_parse( parse->left->left, c, result_location );
            compile_emit_branch( c, INSTR_JUMP, switch_end );
            compile_set_label( c, next_case );
        }
        compile_emit( c, INSTR_POP, 0 );
        adjust_result( c, RESULT_NONE, result_location );
        compile_set_label( c, switch_end );
    }
    else if ( parse->type == PARSE_NULL )
    {
        adjust_result( c, RESULT_NONE, result_location );
    }
    else
    {
        assert( !"unknown PARSE type." );
    }
}

OBJECT * function_rulename( FUNCTION * function )
{
    return function->rulename;
}

void function_set_rulename( FUNCTION * function, OBJECT * rulename )
{
    function->rulename = rulename;
}

void function_location( FUNCTION * function_, OBJECT * * file, int * line )
{
    if ( function_->type == FUNCTION_BUILTIN )
    {
        *file = constant_builtin;
        *line = -1;
    }
    else
    {
        JAM_FUNCTION * function = (JAM_FUNCTION *)function_;
        *file = function->file;
        *line = function->line;
    }
}

FUNCTION * function_builtin( LIST * ( * func )( FRAME * frame, int flags ), int flags )
{
    BUILTIN_FUNCTION * result = BJAM_MALLOC( sizeof( BUILTIN_FUNCTION ) );
    result->type = FUNCTION_BUILTIN;
    result->reference_count = 1;
    result->rulename = 0;
    result->func = func;
    result->flags = flags;
    return (FUNCTION *)result;
}

FUNCTION * function_compile( PARSE * parse )
{
    compiler c[1];
    JAM_FUNCTION * result;
    compiler_init( c );
    compile_parse( parse, c, RESULT_RETURN );
    compile_emit( c, INSTR_RETURN, 0 );
    result = compile_to_function( c );
    compiler_free( c );
    result->file = object_copy( parse->file );
    result->line = parse->line;
    return (FUNCTION *)result;
}

void function_refer( FUNCTION * func )
{
    ++func->reference_count;
}

void function_free( FUNCTION * function_ )
{
    int i;

    if ( --function_->reference_count != 0 ) return;

    if ( function_->rulename ) object_free( function_->rulename );

    if ( function_->type == FUNCTION_JAM )
    {
        JAM_FUNCTION * func = (JAM_FUNCTION *)function_;

        BJAM_FREE( func->code );
        for ( i = 0; i < func->num_constants; ++i )
        {
            object_free( func->constants[i] );
        }
        BJAM_FREE( func->constants );

        for ( i = 0; i < func->num_subfunctions; ++i )
        {
            object_free( func->functions[i].name );
            function_free( func->functions[i].code );
        }
        BJAM_FREE( func->functions );

        for ( i = 0; i < func->num_subactions; ++i )
        {
            object_free( func->actions[i].name );
            object_free( func->actions[i].command );
        }
        BJAM_FREE( func->actions );

        object_free( func->file );
    }

    BJAM_FREE( function_ );
}

/*
 * WARNING: The instruction set is tuned for Jam and
 * is not really generic.  Be especially careful about
 * stack push/pop.
 */

LIST * function_run( FUNCTION * function_, FRAME * frame, STACK * s )
{
    JAM_FUNCTION * function;
    instruction * code;
    LIST * l;
    LIST * r;
    LIST * result = L0;
    void * saved_stack = s->data;

    if ( function_->type == FUNCTION_BUILTIN )
    {
        BUILTIN_FUNCTION * f = (BUILTIN_FUNCTION *)function_;
        return f->func( frame, f->flags );
    }

    function = (JAM_FUNCTION *)function_;
    code = function->code;
    for ( ; ; )
    {
        switch ( code->op_code )
        {

        /*
         * Basic stack manipulation
         */

        case INSTR_PUSH_EMPTY:
        {
            stack_push( s, L0 );
            break;
        }

        case INSTR_PUSH_CONSTANT:
        {
            OBJECT * value = function_get_constant( function, code->arg );
            stack_push( s, list_new( L0, object_copy( value ) ) );
            break;
        }

        case INSTR_PUSH_ARG:
        {
            stack_push( s, frame_get_local( frame, code->arg ) );
            break;
        }

        case INSTR_PUSH_VAR:
        {
            stack_push( s, function_get_variable( function, frame, code->arg ) );
            break;
        }

        case INSTR_PUSH_GROUP:
        {
            LIST * value = L0;
            l = stack_pop( s );
            for ( r = l; r; r = list_next( r ) )
            {
                LIST * one = function_get_named_variable( function, frame, r->value );
                value = list_append( value, one );
            }
            list_free( l );
            stack_push( s, value );
            break;
        }

        case INSTR_PUSH_APPEND:
        {
            r = stack_pop( s );
            l = stack_pop( s );
            stack_push( s, list_append( r, l ) );
            break;
        }

        case INSTR_SWAP:
        {
            l = stack_top( s );
            stack_set( s, 0, stack_at( s, code->arg ) );
            stack_set( s, code->arg, l );
            break;
        }

        case INSTR_POP:
        {
            list_free( stack_pop( s ) );
            break;
        }

        /*
         * Branch instructions
         */

        case INSTR_JUMP:
        {
            code += code->arg;
            break;
        }

        case INSTR_JUMP_EMPTY:
        {
            l = stack_pop( s );
            if ( !list_cmp( l, L0 ) ) { code += code->arg; }
            list_free( l );
            break;
        }

        case INSTR_JUMP_NOT_EMPTY:
        {
            l = stack_pop( s );
            if( list_cmp( l, L0 ) ) { code += code->arg; }
            list_free( l );
            break;
        }

        case INSTR_JUMP_LT:
        {
            r = stack_pop( s );
            l = stack_pop( s );
            if ( list_cmp( l, r ) < 0 ) { code += code->arg; }
            list_free( l );
            list_free( r );
            break;
        }

        case INSTR_JUMP_LE:
        {
            r = stack_pop( s );
            l = stack_pop( s );
            if ( list_cmp( l, r ) <= 0 ) { code += code->arg; }
            list_free( l );
            list_free( r );
            break;
        }

        case INSTR_JUMP_GT:
        {
            r = stack_pop( s );
            l = stack_pop( s );
            if ( list_cmp( l, r ) > 0 ) { code += code->arg; }
            list_free( l );
            list_free( r );
            break;
        }

        case INSTR_JUMP_GE:
        {
            r = stack_pop( s );
            l = stack_pop( s );
            if ( list_cmp( l, r ) >= 0 ) { code += code->arg; }
            list_free( l );
            list_free( r );
            break;
        }

        case INSTR_JUMP_EQ:
        {
            r = stack_pop( s );
            l = stack_pop( s );
            if( list_cmp( l, r ) == 0 ) { code += code->arg; }
            list_free( l );
            list_free( r );
            break;
        }

        case INSTR_JUMP_NE:
        {
            r = stack_pop(s);
            l = stack_pop(s);
            if( list_cmp(l, r) != 0 ) { code += code->arg; }
            list_free(l);
            list_free(r);
            break;
        }

        case INSTR_JUMP_IN:
        {
            r = stack_pop(s);
            l = stack_pop(s);
            if ( list_is_sublist( l, r ) ) { code += code->arg; }
            list_free(l);
            list_free(r);
            break;
        }

        case INSTR_JUMP_NOT_IN:
        {
            r = stack_pop( s );
            l = stack_pop( s );
            if( !list_is_sublist( l, r ) ) { code += code->arg; }
            list_free( l );
            list_free( r );
            break;
        }

        /*
         * For
         */
        
        case INSTR_TRY_POP_FRONT:
        {
            l = stack_pop( s );
            if( !l )
            {
                code += code->arg;
            }
            else
            {
                r = list_new( L0, object_copy( l->value ) );
                l = list_pop_front( l );
                stack_push( s, l );
                stack_push( s, r );
            }
            break;
        }

        /*
         * Switch
         */

        case INSTR_JUMP_NOT_GLOB:
        {
            const char * pattern;
            const char * match;
            l = stack_pop( s );
            r = stack_top( s );
            pattern = l ? object_str( l->value ) : "";
            match = r ? object_str( r->value ) : "";
            if( glob( pattern, match ) )
            {
                code += code->arg;
            }
            else
            {
                list_free( stack_pop( s ) );
            }
            list_free( l );
            break;
        }

        /*
         * Return
         */

        case INSTR_SET_RESULT:
        {
            list_free( result );
            result = stack_pop( s );
            break;
        }

        case INSTR_PUSH_RESULT:
        {
            stack_push( s, result );
            result = L0;
            break;
        }

        case INSTR_RETURN:
        {
            assert( saved_stack == s->data );
            return result;
        }

        /*
         * Local variables
         */

        case INSTR_PUSH_LOCAL:
        {
            LIST * value = stack_pop( s );
            stack_push( s, function_swap_variable( function, frame, code->arg, value ) );
            break;
        }

        case INSTR_POP_LOCAL:
        {
            function_set_variable( function, frame, code->arg, stack_pop( s ) );
            break;
        }

        case INSTR_PUSH_LOCAL_GROUP:
        {
            LIST * value = stack_pop( s );
            l = stack_pop( s );
            for( r = l; r; r = list_next( r ) )
            {
                LIST * saved = function_swap_named_variable( function, frame, r->value, list_copy( L0, value ) );
                stack_push( s, saved );
            }
            list_free( value );
            stack_push( s, l );
            break;
        }

        case INSTR_POP_LOCAL_GROUP:
        {
            r = stack_pop( s );
            l = list_reverse( r );
            list_free( r );
            for( r = l; r; r = list_next( r ) )
            {
                function_set_named_variable( function, frame, r->value, stack_pop( s ) );
            }
            list_free( l );
            break;
        }

        /*
         * on $(TARGET) variables
         */

        case INSTR_PUSH_ON:
        {
            LIST * targets = stack_top( s );
            if ( targets )
            {
                /*
                 * FIXME: push the state onto the stack instead of
                 * using pushsettings.
                 */
                TARGET * t = bindtarget( targets->value );
                pushsettings( t->settings );
            }
            else
            {
                /*
                 * [ on $(TARGET) ... ] is ignored if $(TARGET) is empty.
                 */
                list_free( stack_pop( s ) );
                stack_push( s, L0 );
                code += code->arg;
            }
            break;
        }

        case INSTR_POP_ON:
        {
            LIST * result = stack_pop( s );
            LIST * targets = stack_pop( s );
            if ( targets )
            {
                TARGET * t = bindtarget( targets->value );
                popsettings( t->settings );
            }
            list_free( targets );
            stack_push( s, result );
            break;
        }

        case INSTR_SET_ON:
        {
            LIST * targets = stack_pop( s );
            LIST * value = stack_pop( s );
            LIST * vars = stack_pop( s );
            LIST * ts;
            for ( ts = targets; ts; ts = list_next( ts ) )
            {
                TARGET * t = bindtarget( ts->value );
                LIST   * l;

                for ( l = vars; l; l = list_next( l ) )
                t->settings = addsettings( t->settings, VAR_SET, l->value,
                    list_copy( L0, value ) );
            }
            list_free( vars );
            list_free( targets );
            stack_push( s, value );
            break;
        }

        case INSTR_APPEND_ON:
        {
            LIST * targets = stack_pop( s );
            LIST * value = stack_pop( s );
            LIST * vars = stack_pop( s );
            LIST * ts;
            for ( ts = targets; ts; ts = list_next( ts ) )
            {
                TARGET * t = bindtarget( ts->value );
                LIST   * l;

                for ( l = vars; l; l = list_next( l ) )
                t->settings = addsettings( t->settings, VAR_APPEND, l->value,
                    list_copy( L0, value ) );
            }
            list_free( vars );
            list_free( targets );
            stack_push( s, value );
            break;
        }

        case INSTR_DEFAULT_ON:
        {
            LIST * targets = stack_pop( s );
            LIST * value = stack_pop( s );
            LIST * vars = stack_pop( s );
            LIST * ts;
            for ( ts = targets; ts; ts = list_next( ts ) )
            {
                TARGET * t = bindtarget( ts->value );
                LIST   * l;

                for ( l = vars; l; l = list_next( l ) )
                t->settings = addsettings( t->settings, VAR_DEFAULT, l->value,
                    list_copy( L0, value ) );
            }
            list_free( vars );
            list_free( targets );
            stack_push( s, value );
            break;
        }

        /*
         * Variable setting
         */

        case INSTR_SET:
        {
            function_set_variable( function, frame, code->arg, list_copy( L0, stack_top( s ) ) );
            break;
        }

        case INSTR_APPEND:
        {
            function_append_variable( function, frame, code->arg, list_copy( L0, stack_top( s ) ) );
            break;
        }
        case INSTR_DEFAULT:
        {
            function_default_variable( function, frame, code->arg, list_copy( L0, stack_top( s ) ) );
            break;
        }

        case INSTR_SET_GROUP:
        {
            LIST * value = stack_pop( s );
            LIST * vars = stack_pop( s );
            for( r = vars; r; r = list_next( r ) )
                function_set_named_variable( function, frame, r->value, list_copy( L0, value ) );
            list_free( vars );
            stack_push( s, value );
            break;
        }

        case INSTR_APPEND_GROUP:
        {
            LIST * value = stack_pop( s );
            LIST * vars = stack_pop( s );
            for( r = vars; r; r = list_next( r ) )
                function_append_named_variable( function, frame, r->value, list_copy( L0, value ) );
            list_free( vars );
            stack_push( s, value );
            break;
        }

        case INSTR_DEFAULT_GROUP:
        {
            LIST * value = stack_pop( s );
            LIST * vars = stack_pop( s );
            for( r = vars; r; r = list_next( r ) )
                function_default_named_variable( function, frame, r->value, list_copy( L0, value ) );
            list_free( vars );
            stack_push( s, value );
            break;
        }

        /*
         * Rules
         */

        case INSTR_CALL_RULE:
        {
            const char * unexpanded =
                object_str( function_get_constant( function, code[1].op_code ) );
            LIST * result = function_call_rule( function, frame, s, code->arg, unexpanded, function->file, code[1].arg );
            stack_push( s, result );
            ++code;
            break;
        }

        case INSTR_RULE:
        {
            function_set_rule( function, frame, s, code->arg );
            break;
        }

        case INSTR_ACTIONS:
        {
            function_set_actions( function, frame, s, code->arg );
            break;
        }

        /*
         * Variable expansion
         */

        case INSTR_APPLY_MODIFIERS:
        {
            int n;
            int i;
            l = stack_pop( s );
            n = expand_modifiers( s, code->arg );
            stack_push( s, l );
            l = apply_modifiers( s, n );
            list_free( stack_pop( s ) );
            stack_deallocate( s, n * sizeof( VAR_EDITS ) );
            for ( i = 0; i < code->arg; ++i )
                list_free( stack_pop( s ) ); /* pop modifiers */
            stack_push( s, l );
            break;
        }
                                    
        case INSTR_APPLY_INDEX:
        {
            l = apply_subscript( s );
            list_free( stack_pop( s ) );
            list_free( stack_pop( s ) );
            stack_push( s, l );
            break;
        }
                                    
        case INSTR_APPLY_INDEX_MODIFIERS:
        {
            int i;
            int n;
            l = stack_pop( s );
            r = stack_pop( s );
            n = expand_modifiers( s, code->arg );
            stack_push( s, r );
            stack_push( s, l );
            l = apply_subscript_and_modifiers( s, n );
            list_free( stack_pop( s ) );
            list_free( stack_pop( s ) );
            stack_deallocate( s, n * sizeof( VAR_EDITS ) );
            for ( i = 0; i < code->arg; ++i )
                list_free( stack_pop( s ) ); /* pop modifiers */
            stack_push( s, l );
            break;
        }

        case INSTR_APPLY_MODIFIERS_GROUP:
        {
            int i;
            LIST * vars = stack_pop( s );
            int n = expand_modifiers( s, code->arg );
            LIST * result = L0;
            for( l = vars; l; l = list_next( l ) )
            {
                stack_push( s, function_get_named_variable( function, frame, l->value ) );
                result = list_append( result, apply_modifiers( s, n ) );
                list_free( stack_pop( s ) );
            }
            list_free( vars );
            stack_deallocate( s, n * sizeof( VAR_EDITS ) );
            for ( i = 0; i < code->arg; ++i )
                list_free( stack_pop( s ) ); /* pop modifiers */
            stack_push( s, result );
            break;
        }
                                    
        case INSTR_APPLY_INDEX_GROUP:
        {
            LIST * vars = stack_pop( s );
            LIST * result = L0;
            for( l = vars; l; l = list_next( l ) )
            {
                stack_push( s, function_get_named_variable( function, frame, l->value ) );
                result = list_append( result, apply_subscript( s ) );
                list_free( stack_pop( s ) );
            }
            list_free( vars );
            list_free( stack_pop( s ) );
            stack_push( s, result );
            break;
        }
                                    
        case INSTR_APPLY_INDEX_MODIFIERS_GROUP:
        {
            int i;
            LIST * vars = stack_pop( s );
            LIST * r = stack_pop( s );
            int n = expand_modifiers( s, code->arg );
            LIST * result = L0;
            stack_push( s, r );
            for( l = vars; l; l = list_next( l ) )
            {
                stack_push( s, function_get_named_variable( function, frame, l->value ) );
                result = list_append( result, apply_subscript_and_modifiers( s, n ) );
                list_free( stack_pop( s ) );
            }
            list_free( stack_pop( s ) );
            list_free( vars );
            stack_deallocate( s, n * sizeof( VAR_EDITS ) );
            for ( i = 0; i < code->arg; ++i )
                list_free( stack_pop( s ) ); /* pop modifiers */
            stack_push( s, result );
            break;
        }

        case INSTR_COMBINE_STRINGS:
        {
            LIST * result;
            size_t buffer_size = code->arg * sizeof( expansion_item );
            LIST * * stack_pos = stack_get( s );
            expansion_item * items = stack_allocate( s, buffer_size );
            int i;
            for( i = 0; i < code->arg; ++i )
            {
                items[i].elem = items[i].saved = stack_pos[i];
            }
            result = expand( items, code->arg );
            stack_deallocate( s, buffer_size );
            for( i = 0; i < code->arg; ++i )
            {
                list_free( stack_pop( s ) );
            }
            stack_push( s, result );
            break;
        }

        case INSTR_INCLUDE:
        {
            LIST * nt = stack_pop( s );

            if ( nt )
            {
                TARGET * t = bindtarget( nt->value );
                list_free( nt );

                /* DWA 2001/10/22 - Perforce Jam cleared the arguments here, which
                 * prevents an included file from being treated as part of the body of a
                 * rule. I did not see any reason to do that, so I lifted the
                 * restriction.
                 */

                /* Bind the include file under the influence of */
                /* "on-target" variables.  Though they are targets, */
                /* include files are not built with make(). */

                pushsettings( t->settings );
                /* We don't expect that file to be included is generated by some
                   action. Therefore, pass 0 as third argument.
                   If the name resolves to directory, let it error out.  */
                object_free( t->boundname );
                t->boundname = search( t->name, &t->time, 0, 0 );
                popsettings( t->settings );

                parse_file( t->boundname, frame );
            }

            break;
        }

        /*
         * Classes and modules
         */

        case INSTR_PUSH_MODULE:
        {
            LIST * module_name = stack_pop( s );

            module_t * outer_module = frame->module;
            frame->module = module_name ? bindmodule( module_name->value ) : root_module();

            list_free( module_name );

            if ( outer_module != frame->module )
            {
                exit_module( outer_module );
                enter_module( frame->module );
            }

            *(module_t * *)stack_allocate( s, sizeof( module_t * ) ) = outer_module;

            break;
        }

        case INSTR_POP_MODULE:
        {
            module_t * outer_module = *(module_t * *)stack_get( s );
            stack_deallocate( s, sizeof( module_t * ) );
            if ( outer_module != frame->module )
            {
                exit_module( frame->module );
                enter_module( outer_module );
                frame->module = outer_module;
            }
            break;
        }

        case INSTR_CLASS:
        {
            LIST * bases = stack_pop( s );
            LIST * name = stack_pop( s );
            OBJECT * class_module = make_class_module( name, bases, frame );

            module_t * outer_module = frame->module;
            frame->module = bindmodule( class_module );
            object_free( class_module );

            if ( outer_module != frame->module )
            {
                exit_module( outer_module );
                enter_module( frame->module );
            }
            
            *(module_t * *)stack_allocate( s, sizeof( module_t * ) ) = outer_module;

            break;
        }
        
        case INSTR_APPEND_STRINGS:
        {
            string buf[1];
            string_new( buf );
            combine_strings( s, code->arg, buf );
            stack_push( s, list_new( L0, object_new( buf->value ) ) );
            string_free( buf );
            break;
        }
        
        case INSTR_WRITE_FILE:
        {
            string buf[1];
            const char * out;
            LIST * filename;
            int out_debug = DEBUG_EXEC ? 1 : 0;
            FILE * out_file = 0;
            string_new( buf );
            combine_strings( s, code->arg, buf );
            filename = stack_top( s );
            out = object_str( filename->value );
            if ( !globs.noexec )
            {
                string out_name[1];
                /* Handle "path to file" filenames. */
                if ( ( out[ 0 ] == '"' ) && ( out[ strlen( out ) - 1 ] == '"' ) )
                {
                    string_copy( out_name, out + 1 );
                    string_truncate( out_name, out_name->size - 1 );
                }
                else
                {
                    string_copy( out_name, out );
                }
                out_file = fopen( out_name->value, "w" );
            
                if ( !out_file )
                {
                    printf( "failed to write output file '%s'!\n", out_name->value );
                    exit( EXITBAD );
                }
                string_free( out_name );
            }

            if ( out_debug ) printf( "\nfile %s\n", out );

            if ( out_file ) fputs( buf->value, out_file );
            if ( out_debug ) puts( buf->value );
            
            fflush( out_file );
            fclose( out_file );
            string_free( buf );

            if ( out_debug ) fputc( '\n', stdout );

            break;
        }

        }
        ++code;
    }
}

void function_done( void )
{
    BJAM_FREE( stack );
}
