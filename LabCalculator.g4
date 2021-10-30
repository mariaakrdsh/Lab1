grammar LabCalculator;

/*
 * Parser Rules
 */

compileUnit : expression EOF;

expression :
	LPAREN expression RPAREN #ParenthesizedExpr
	| expression EXPONENT expression #ExponentialExpr
    | expression operatorToken=(MULTIPLY | DIVIDE) expression #MultiplicativeExpr
	| expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr 
	| expression operatorToken=(DIV | MOD) expression #ModDivExpr
	| SUBSTRACT LPAREN expression RPAREN #UnarExpr
	| tokenOperator=(NUMBER | COMMA) expression #BigExpr
	| INC LPAREN expression RPAREN #IncExpr
	| DEC LPAREN expression RPAREN #DecExpr
	| NUMBER #NumberExpr 
	| tokenOperator=(MAX | MIN) LPAREN expression COMMA expression RPAREN #MaxMinExpr
	| IDENTIFIER #IdentifierExpr
	; 
	

/*
 * Lexer Rules
 */

NUMBER : INT ('.' INT)?; 
IDENTIFIER : [a-zA-Z]+[1-9][0-9]+;

INT : ('0'..'9')+;

EXPONENT : '^';
MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';
LPAREN : '(';
RPAREN : ')';
COMMA : ',';
DIV : 'div';
MOD : 'mod';
MAX : 'max';
MIN : 'min';
INC : 'inc';
DEC : 'dec';


WS : [ \t\r\n] -> channel(HIDDEN);