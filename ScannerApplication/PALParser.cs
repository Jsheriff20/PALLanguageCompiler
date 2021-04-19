using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AllanMilne.Ardkit;
using AllanMilne.PALCompiler;

namespace PALLanguageCompiler
{
    class PALParser : RecoveringRdParser
    {

        PALSemantics semantics;
        public PALParser() : base(new PALScanner())
        {
            semantics = new PALSemantics(this);
        }


        protected override void recStarter()
        {
            Scope.OpenScope();
            mustBe("PROGRAM");
            mustBe(Token.IdentifierToken);
            mustBe("WITH");
            recVarDecls();
            mustBe("IN");

            do
            {
                //in the event of an endless loop and the exit token is not found, exit manually
                if (!have(Token.IdentifierToken) && !have("UNTIL") && !have("IF") && !have("INPUT") && !have("OUTPUT") && !have("END"))
                {
                    break;
                }
                else
                {
                    recStatement();
                }
            }
            while (!have("END"));

            mustBe("END");
            Scope.CloseScope();
        }


        private void recVarDecls()
        {
            while (have(Token.IdentifierToken))
            {
                List<IToken> tokens = recIdentList();
                mustBe("AS");
                int languageType = recType();

                foreach (IToken token in tokens)
                {

                    ConstSymbol symbol = new ConstSymbol(token, languageType);
                    semantics.AddToScope(symbol);
                }
            }
        }


        private int recType()
        {
            if (have("REAL"))
            {
                mustBe("REAL");
                return LanguageType.Real;
            }
            else if (have("INTEGER"))
            {
                mustBe("INTEGER");
                return LanguageType.Integer;
            }
            else
            {
                syntaxError("Error with TYPE");
                //return 0, as 0 is undefined
                return 0;
            }
        }


        private void recStatement()
        {
            if (have(Token.IdentifierToken))
            {
                recAssignment();
            }
            else if (have("UNTIL"))
            {
                recLoop();
            }
            else if (have("IF"))
            {
                recConditional();
            }
            else if (have("INPUT") || have("OUTPUT"))
            {
                recIO();
            }
            else
            {
                syntaxError("Error with STATEMENT");
            }
        }


        private void recAssignment()
        {
            int type1 = semantics.CheckId(scanner.CurrentToken);

            mustBe(Token.IdentifierToken);
            mustBe("=");

            int type2 = recExpression();
            semantics.CompareTypes(scanner.CurrentToken, type1, type2);

        }


        private void recLoop()
        {
            mustBe("UNTIL");
            recBooleanExpr();
            mustBe("REPEAT");

            while (!have("ENDLOOP"))
            {
                //in the event of an endless loop and the exit token is not found, exit manually
                if (!have(Token.IdentifierToken) && !have("UNTIL") && !have("IF") && !have("INPUT") && !have("OUTPUT") && !have("ENDLOOP"))
                {
                    break;
                }
                else
                {
                    recStatement();
                }
            }
            mustBe("ENDLOOP");
        }


        private void recConditional()
        {
            mustBe("IF");
            recBooleanExpr();
            mustBe("THEN");

            while (!have("ELSE") && !have("ENDIF"))
            {
                recStatement();
            }

            if (have("ELSE"))
            {
                mustBe("ELSE");

                while (!have("ENDIF"))
                {
                    //in the event of an endless loop and the exit token is not found, exit manually
                    if (!have(Token.IdentifierToken) && !have("UNTIL") && !have("IF") && !have("INPUT") && !have("OUTPUT") && !have("ENDIF"))
                    {
                        break;
                    }
                    else
                    {
                        recStatement();
                    }
                }
            }

            mustBe("ENDIF");
        }


        private void recIO()
        {
            if (have("INPUT"))
            {
                mustBe("INPUT");
                recIdentList();
            }
            else if (have("OUTPUT"))
            {
                mustBe("OUTPUT");
                recExpression();

                while (have(","))
                {
                    mustBe(",");
                    recExpression();
                }
            }
            else
            {
                syntaxError("Error with IO");

            }

        }


        private int recExpression()
        {
            //if this type is different from the other types then an error will occur later
            //if there is no error with this type then this can represent the type of the whole 
            int type1 = recTerm();
            while (have("-") || have("+"))
            {
                if (have("+"))
                {
                    mustBe("+");
                }
                else if (have("-"))
                {
                    mustBe("-");
                }
                else
                {
                    syntaxError("Expected + or -");
                }

                //check if the types are all the same
                int type2 = recTerm();
                semantics.CompareTypes(scanner.CurrentToken, type1, type2);
            }

            return type1;
        }


        private int recTerm()
        {
            //if this type is different from the other types then an error will occur later
            //if there is no error with this type then this can represent the type of the whole 
            int type1 = recFactor();

            while (have("*") || have("/"))
            {
                if (have("*"))
                {
                    mustBe("*");
                }
                else if (have("/"))
                {
                    mustBe("/");
                }
                else
                {
                    syntaxError("Expected * or /");
                }

                //check if the types are all the same
                int type2 = recFactor();
                semantics.CompareTypes(scanner.CurrentToken, type1, type2);
            }

            return type1;
        }


        private int recFactor()
        {
            //return 0 as 0 is undefined
            int type = 0;

            //optional + or -
            if (have("+"))
            {
                mustBe("+");
            }
            else if (have("-"))
            {
                mustBe("-");
            }

            if (have("("))
            {
                mustBe("(");
                type = recExpression();
                mustBe(")");
            }
            else if (have(Token.IdentifierToken) || have(Token.IntegerToken) || have(Token.RealToken))
            {
                type = recValue();
            }
            else
            {
                syntaxError("Expected a value or expression");
            }
            return type;
        }


        private int recValue()
        {

            //return 0 as 0 is undefined
            int type = 0;

            if (have(Token.IdentifierToken))
            {
                type = semantics.CheckId(scanner.CurrentToken);
                mustBe(Token.IdentifierToken);
            }
            else if (have(Token.IntegerToken))
            {
                type = LanguageType.Integer;
                mustBe(Token.IntegerToken);
            }
            else if (have(Token.RealToken))
            {
                type = LanguageType.Real;
                mustBe(Token.RealToken);
            }
            else
            {
                syntaxError("Value not recognised");
            }

            return type;

        }


        private void recBooleanExpr()
        {
            int type1 = recExpression();
            if (have("<"))
            {
                mustBe("<");
            }
            else if (have("="))
            {
                mustBe("=");
            }
            else if (have(">"))
            {
                mustBe(">");
            }
            else
            {
                syntaxError("Expected either <, = or >");
            }

            int type2 = recExpression();

            semantics.CompareTypes(scanner.CurrentToken, type1, type2);
        }


        private List<IToken> recIdentList()
        {
            List<IToken> tokens = new List<IToken>();

            tokens.Add(scanner.CurrentToken);
            mustBe(Token.IdentifierToken);

            while (have(","))
            {
                mustBe(",");
                tokens.Add(scanner.CurrentToken);
                mustBe(Token.IdentifierToken);
            }

            return tokens;
        }

    }
}
