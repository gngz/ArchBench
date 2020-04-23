grammar ArchBench;

commands    : command NEWLINE
            ;

command     : Start PORT
            | STOP
            | INSTALL PATH
            // | 'with' ( ID | IDENTIFIER ) 'set' IDENTIFIER '=' VALUE
            // | 'enable' ( ID | IDENTIFIER )
            // | 'disable' ( ID | IDENTIFIER )
            // | 'show' ( | ID | IDENTIFIER )
            // | 'exit'
            ;

// VALUE       : ID
//             | IDENTIFIER 
//             ;

// IDENTIFIER  : LETTER ( LETTER | DIGIT | SYMBOL )*
//             | PRIME LETTER ( LETTER | DIGIT | SYMBOL | SPACE )+ PRIME
//             ;

// ID          : DIGIT+
//             ;

// SYMBOL      : [_?]
//             ;

// SPACE       : [ ]
//             ;

// PRIME       : [']
//             ;

// LETTER      : [A-Za-z]
//             ;

// DIGIT       : [0-9]
//             ;

PORT        : [0-9]+
            ;

PATH        : [A-Za-z_.:\\/]+
            ;

EXIT        : 'exit'
            ;

fragment Start  : 'start'
                ;

STOP        : 'stop'
            ;

INSTALL     : 'install'
            ;

NEWLINE     : [\n]
            ;

WHITESPACE  : [ \t\r]+ -> skip
            ;

