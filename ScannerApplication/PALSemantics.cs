using System;
using System.Collections.Generic;
using System.Text;
using AllanMilne.Ardkit;

namespace PALLanguageCompiler
{
    class PALSemantics : Semantics
    {

        public PALSemantics(IParser parser) : base(parser)
        {

        }

        public int CheckId(IToken id)
        {
            if (!id.Is(Token.IdentifierToken))
            {
                return LanguageType.Undefined;
            }

            if (!Scope.CurrentScope.IsDefined(id.TokenValue))
            {
                semanticError(new NotDeclaredError(id));
                return LanguageType.Undefined;
            }


            return Scope.CurrentScope.Get(id.TokenValue).Type;

        }


        public void CompareTypes(IToken token, int oldType, int newType)
        {
            if (newType != oldType)
            {
                semanticError(new TypeConflictError
                (token, newType, oldType));
            }
        }

        public void AddToScope(ISymbol symbol)
        {
            if (Scope.CurrentScope.IsDefined(symbol.Name))
            {
                semanticError(new AlreadyDeclaredError(symbol.Source, symbol));
            }
            else
            {
                Scope.CurrentScope.Add(symbol);
            }
        }
    }
}
