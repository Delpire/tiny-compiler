# tiny-compiler
Learning how to write a compiler

This repo contains a compiler for a programming language called `tiny`. It was created following this [blog post](https://austinhenley.com/blog/teenytinycompiler1.html).

The compiler was created in .NET, and transpiles `.tiny` files to `.c` files.

`tiny-compiler` supports the following:
- Printing
- Taking inputs an floats
- Defining variables
- While loops
- If statements
- Basic operations like addition, subtraction, multiplication, and division.
- Labels and GOTO.

Here is an example `tiny` program.
```
# Compute average of given values.

LET a = 0
WHILE a < 1 REPEAT
    PRINT "Enter number of scores: "
    INPUT a
ENDWHILE

LET b = 0
LET s = 0
PRINT "Enter one value at a time: "
WHILE b < a REPEAT
    INPUT c
    LET s = s + c
    LET b = b + 1
ENDWHILE

PRINT "Average: "
PRINT s / a
```
