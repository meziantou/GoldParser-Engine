using System;
using System.IO;

namespace GoldParserEngine
{
    /// <summary>
    ///     This is the main class in the GOLD Parser Engine and is used to perform
    ///     all duties required to the parsing of a source text string. This class
    ///     contains the LALR(1) State Machine code, the DFA State Machine code,
    ///     character table (used by the DFA algorithm) and all other structures and
    ///     methods needed to interact with the developer.
    /// </summary>
    public class Parser
    {
        private const string Version = "5.0";
        private readonly Position _currentPosition = new Position();

        private readonly SymbolList _expectedSymbols = new SymbolList();
        private readonly TokenStack _groupStack = new TokenStack();
        private readonly TokenStack _inputTokens = new TokenStack();
        private readonly TokenStack _stack = new TokenStack();

        private readonly Position _sysPosition = new Position();
        private bool _areTablesLoaded;
        private CharacterSetList _charSetTable = new CharacterSetList();
        private int _currentLALR;
        private FAStateList _dfa = new FAStateList();

        private GrammarProperties _grammar = new GrammarProperties();
        private GroupList _groupTable = new GroupList();
        private bool _haveReduction;
        private string _lookaheadBuffer;
        private LRStateList _lrStates = new LRStateList();
        private ProductionList _productionTable = new ProductionList();
        private TextReader _source;
        private SymbolList _symbolTable = new SymbolList();

        public Parser()
        {
            Restart();
        }

        /// <summary>
        ///     When the Parse() method returns a Reduce, this method will contain the current Reduction.
        /// </summary>
        /// <returns></returns>
        public object CurrentReduction
        {
            get
            {
                if (_haveReduction)
                {
                    return _stack.Peek().Data;
                }

                return null;
            }
            set
            {
                if (_haveReduction)
                {
                    _stack.Peek().Data = value;
                }
            }
        }

        /// <summary>
        ///     Determines if reductions will be trimmed in cases where a production contains a single element.
        /// </summary>
        /// <returns></returns>
        public bool TrimReductions { get; set; }

        /// <summary>
        ///     Returns information about the current grammar.
        /// </summary>
        /// <returns></returns>
        public GrammarProperties Grammar
        {
            get { return _grammar; }
        }

        /// <summary>
        ///     Current line and column being read from the source.
        /// </summary>
        /// <returns></returns>
        public Position CurrentPosition
        {
            get { return _currentPosition; }
        }

        /// <summary>
        ///     If the Parse() function returns TokenRead, this method will return that last read token.
        /// </summary>
        /// <returns></returns>
        public Token CurrentToken
        {
            get { return _inputTokens.Peek(); }
        }

        public string CurrentTokenString
        {
            get
            {
                if (CurrentToken == null)
                    return null;

                return CurrentToken.Data as string;
            }
        }

        /// <summary>
        ///     Returns a list of Symbols recognized by the grammar.
        /// </summary>
        /// <returns></returns>
        public SymbolList SymbolTable
        {
            get { return _symbolTable; }
        }

        /// <summary>
        ///     Returns a list of Productions recognized by the grammar.
        /// </summary>
        /// <returns></returns>
        public ProductionList ProductionTable
        {
            get { return _productionTable; }
        }

        /// <summary>
        ///     If the Parse() method returns a SyntaxError, this method will contain a list of the symbols the grammar expected to
        ///     see.
        /// </summary>
        /// <returns></returns>
        public SymbolList ExpectedSymbols
        {
            get { return _expectedSymbols; }
        }

        /// <summary>
        ///     Returns true if parse tables were loaded.
        /// </summary>
        /// <returns></returns>
        public bool AreTablesLoaded
        {
            get { return _areTablesLoaded; }
        }

        /// <summary>
        ///     Opens a string for parsing.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public bool Open(string text)
        {
            return Open(new StringReader(text));
        }

        /// <summary>
        ///     Opens a text stream for parsing.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool Open(TextReader reader)
        {
            var start = new Token();

            Restart();
            _source = reader;

            //=== Create stack top item. Only needs state
            start.State = _lrStates.InitialState;
            _stack.Push(start);

            return true;
        }


        /// <summary>
        ///     Removes the next token from the input queue.
        /// </summary>
        /// <returns></returns>
        public Token DiscardCurrentToken()
        {
            return _inputTokens.Pop();
        }

        /// <summary>
        ///     Added a token onto the end of the input queue.
        /// </summary>
        /// <param name="token"></param>
        public void EnqueueInput(Token token)
        {
            _inputTokens.Push(token);
        }

        /// <summary>
        ///     Pushes the token onto the top of the input queue. This token will be analyzed next.
        /// </summary>
        /// <param name="token"></param>
        public void PushInput(Token token)
        {
            _inputTokens.Push(token);
        }

        /// <summary>
        ///     Return Count characters from the lookahead buffer. DO NOT CONSUME
        ///     This is used to create the text stored in a token. It is disgarded
        ///     separately. Because of the design of the DFA algorithm, count should
        ///     never exceed the buffer length. The If-Statement below is fault-tolerate
        ///     programming, but not necessary.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private string LookaheadBuffer(int count)
        {
            if (count > _lookaheadBuffer.Length)
            {
                count = _lookaheadBuffer.Length;
            }

            return _lookaheadBuffer.Substring(0, count);
        }

        /// <summary>
        ///     Return single char at the index. This function will also increase
        ///     buffer if the specified character is not present. It is used
        ///     by the DFA algorithm.
        /// </summary>
        /// <param name="charIndex"></param>
        /// <returns></returns>
        private string Lookahead(int charIndex)
        {
            //Check if we must read characters from the Stream
            if (charIndex > _lookaheadBuffer.Length)
            {
                for (int n = 0; n < charIndex - _lookaheadBuffer.Length; n++)
                {
                    _lookaheadBuffer += (char) (_source.Read());
                }
            }

            //If the buffer is still smaller than the index, we have reached
            //the end of the text. In this case, return a null string - the DFA
            //code will understand.
            if (charIndex <= _lookaheadBuffer.Length)
            {
                return char.ToString(_lookaheadBuffer[(charIndex - 1)]);
            }

            return "";
        }

        /// <summary>
        ///     Library name and version.
        /// </summary>
        /// <returns></returns>
        public string About()
        {
            return "GOLD Parser Engine; Version " + Version;
        }

        internal void Clear()
        {
            _symbolTable.Clear();
            _productionTable.Clear();
            _charSetTable.Clear();
            _dfa.Clear();
            _lrStates.Clear();

            _stack.Clear();
            _inputTokens.Clear();

            _grammar = new GrammarProperties();

            _groupStack.Clear();
            _groupTable.Clear();

            Restart();
        }

        /// <summary>
        ///     Loads parse tables from the specified filename. Only EGT (version 5.0) is supported.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool LoadTables(string path)
        {
            using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    return LoadTables(binaryReader);
                }
            }
        }

        /// <summary>
        ///     Loads parse tables from the specified BinaryReader. Only EGT (version 5.0) is supported.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool LoadTables(BinaryReader reader)
        {
            var egt = new EGTReader();
            try
            {
                egt.Open(reader);

                Restart();
                while (!egt.EndOfFile())
                {
                    egt.GetNextRecord();
                    var recordType = (EGTRecord) egt.RetrieveByte();

                    if (recordType == EGTRecord.Property)
                    {
                        //Index, Name, ValueExpression
                        int index = egt.RetrieveInt16();
                        egt.RetrieveString();
                        //Just discard
                        _grammar.SetValue(index, egt.RetrieveString());
                    }
                    else if (recordType == EGTRecord.TableCounts)
                    {
                        //Symbol, CharacterSet, Rule, DFA, LALR
                        _symbolTable = new SymbolList(egt.RetrieveInt16());
                        _charSetTable = new CharacterSetList(egt.RetrieveInt16());
                        _productionTable = new ProductionList(egt.RetrieveInt16());
                        _dfa = new FAStateList(egt.RetrieveInt16());
                        _lrStates = new LRStateList(egt.RetrieveInt16());
                        _groupTable = new GroupList(egt.RetrieveInt16());
                    }
                    else if (recordType == EGTRecord.InitialStates)
                    {
                        //DFA, LALR
                        _dfa.InitialState = (short) egt.RetrieveInt16();
                        _lrStates.InitialState = (short) egt.RetrieveInt16();
                    }
                    else if (recordType == EGTRecord.Symbol)
                    {
                        //#, Name, Kind

                        int index = egt.RetrieveInt16();
                        string name = egt.RetrieveString();
                        var type = (SymbolType) egt.RetrieveInt16();

                        _symbolTable[index] = new Symbol(name, type, (short) index);
                    }
                    else if (recordType == EGTRecord.Group)
                    {
                        //#, Name, Container#, Start#, End#, Tokenized, Open Ended, Reserved, Count, (Nested Group #...) 
                        var group = new Group();

                        int index = egt.RetrieveInt16();
                        group.Name = egt.RetrieveString();
                        group.Container = SymbolTable[egt.RetrieveInt16()];
                        group.Start = SymbolTable[egt.RetrieveInt16()];
                        group.End = SymbolTable[egt.RetrieveInt16()];

                        group.Advance = (Group.AdvanceMode) egt.RetrieveInt16();
                        group.Ending = (Group.EndingMode) egt.RetrieveInt16();
                        egt.RetrieveEntry();
                        //Reserved
                        int count = egt.RetrieveInt16();
                        for (int n = 0; n < count; n++)
                        {
                            group.Nesting.Add(egt.RetrieveInt16());
                        }


                        //=== Link back
                        group.Container.Group = group;
                        group.Start.Group = group;
                        group.End.Group = group;

                        _groupTable[index] = group;
                    }
                    else if (recordType == EGTRecord.CharRanges)
                    {
                        //#, Total Sets, RESERVED, (Start#, End#  ...)

                        int index = egt.RetrieveInt16();
                        egt.RetrieveInt16();
                        egt.RetrieveInt16();
                        egt.RetrieveEntry();

                        _charSetTable[index] = new CharacterSet();
                        while (!egt.RecordComplete())
                        {
                            _charSetTable[index].Add(new CharacterRange((ushort) egt.RetrieveInt16(), (ushort) egt.RetrieveInt16()));
                        }
                    }
                    else if (recordType == EGTRecord.Production)
                    {
                        //#, ID#, Reserved, (Symbol#,  ...)

                        int index = egt.RetrieveInt16();
                        int headIndex = egt.RetrieveInt16();
                        egt.RetrieveEntry();

                        _productionTable[index] = new Production(_symbolTable[headIndex], (short) index);

                        while (!(egt.RecordComplete()))
                        {
                            int symbolIndex = egt.RetrieveInt16();
                            _productionTable[index].Handle.Add(_symbolTable[symbolIndex]);
                        }
                    }
                    else if (recordType == EGTRecord.DfaState)
                    {
                        //#, Accept?, Accept#, Reserved (CharSet#, Target#, Reserved)...

                        int index = egt.RetrieveInt16();
                        bool accept = egt.RetrieveBoolean();
                        int acceptIndex = egt.RetrieveInt16();
                        egt.RetrieveEntry();

                        if (accept)
                        {
                            _dfa[index] = new FAState(_symbolTable[acceptIndex]);
                        }
                        else
                        {
                            _dfa[index] = new FAState();
                        }

                        //(Edge chars, Target#, Reserved)...
                        while (!egt.RecordComplete())
                        {
                            int setIndex = egt.RetrieveInt16();
                            int target = egt.RetrieveInt16();
                            egt.RetrieveEntry();

                            _dfa[index].Edges.Add(new FAEdge(_charSetTable[setIndex], target));
                        }
                    }
                    else if (recordType == EGTRecord.LrState)
                    {
                        //#, Reserved (Symbol#, Action, Target#, Reserved)...

                        int index = egt.RetrieveInt16();
                        egt.RetrieveEntry();

                        _lrStates[index] = new LRState();

                        //(Symbol#, Action, Target#, Reserved)...
                        while (!(egt.RecordComplete()))
                        {
                            int symbolIndex = egt.RetrieveInt16();
                            int action = egt.RetrieveInt16();
                            int target = egt.RetrieveInt16();
                            egt.RetrieveEntry();

                            _lrStates[index].Add(new LRAction(_symbolTable[symbolIndex], (LRActionType) action, (short) target));
                        }
                    }
                    else
                    {
                        throw new EngineException("File Error. A record of type '" + recordType + "' was read. This is not a valid code.");
                    }
                }
            }
            catch (EngineException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new EngineException("Error while loading tables.", ex);
            }

            _areTablesLoaded = true;
            return true;
        }

        /// <summary>
        ///     This function analyzes a token and either:
        ///     1. Makes a SINGLE reduction and pushes a complete Reduction object on the _stack
        ///     2. Accepts the token and shifts
        ///     3. Errors and places the expected symbol indexes in the Tokens list
        ///     The Token is assumed to be valid and WILL be checked
        ///     If an action is performed that requires controlt to be returned to the user, the function returns true.
        ///     The Message parameter is then set to the type of action.
        /// </summary>
        /// <param name="nextToken"></param>
        /// <returns></returns>
        private ParseResult ParseLALR(Token nextToken)
        {
            ParseResult result = default(ParseResult);
            LRAction parseAction = _lrStates[_currentLALR][(nextToken.Parent)];

            // Work - shift or reduce
            if ((parseAction != null))
            {
                _haveReduction = false;
                //Will be set true if a reduction is made
                //'Debug.WriteLine("Action: " & ParseAction.Text)

                switch (parseAction.Type)
                {
                    case LRActionType.Accept:
                        _haveReduction = true;
                        result = ParseResult.Accept;

                        break;
                    case LRActionType.Shift:
                        _currentLALR = parseAction.Value;
                        nextToken.State = (short) _currentLALR;
                        _stack.Push(nextToken);
                        result = ParseResult.Shift;

                        break;
                    case LRActionType.Reduce:
                        //Produce a reduction - remove as many tokens as members in the rule & push a nonterminal token
                        Production prod = _productionTable[parseAction.Value];

                        //======== Create Reduction
                        Token head;
                        int n;
                        if (TrimReductions && prod.ContainsOneNonTerminal())
                        {
                            //The current rule only consists of a single nonterminal and can be trimmed from the
                            //parse tree. Usually we create a new Reduction, assign it to the Data property
                            //of Head and push it on the _stack. However, in this case, the Data property of the
                            //Head will be assigned the Data property of the reduced token (i.e. the only one
                            //on the _stack).
                            //In this case, to save code, the value popped of the _stack is changed into the head.

                            head = _stack.Pop();
                            head.Parent = prod.Head;

                            result = ParseResult.ReduceEliminated;
                            //Build a Reduction
                        }
                        else
                        {
                            _haveReduction = true;
                            var newReduction = new Reduction(prod.Handle.Count);

                            newReduction.Parent = prod;
                            for (n = prod.Handle.Count - 1; n >= 0; n += -1)
                            {
                                newReduction[n] = _stack.Pop();
                            }

                            head = new Token(prod.Head, newReduction);
                            result = ParseResult.ReduceNormal;
                        }

                        //========== Goto
                        short index = _stack.Peek().State;

                        //========= If n is -1 here, then we have an Internal Table Error!!!!
                        n = _lrStates[index].IndexOf(prod.Head);
                        if (n != -1)
                        {
                            _currentLALR = _lrStates[index][n].Value;

                            head.State = (short) _currentLALR;
                            _stack.Push(head);
                        }
                        else
                        {
                            result = ParseResult.InternalError;
                        }
                        break;
                }
            }
            else
            {
                //=== Syntax Error! Fill Expected Tokens
                _expectedSymbols.Clear();
                //.Count - 1
                foreach (LRAction action in _lrStates[_currentLALR])
                {
                    switch (action.Symbol.Type)
                    {
                        case SymbolType.Terminal:
                        case SymbolType.End:
                        case SymbolType.GroupStart:
                        case SymbolType.GroupEnd:
                            _expectedSymbols.Add(action.Symbol);
                            break;
                    }
                }
                result = ParseResult.SyntaxError;
            }

            return result;
        }

        /// <summary>
        ///     Restarts the parser. Loaded tables are retained.
        /// </summary>
        public void Restart()
        {
            _currentLALR = _lrStates.InitialState;

            //=== Lexer
            _sysPosition.Column = 0;
            _sysPosition.Line = 0;
            _currentPosition.Line = 0;
            _currentPosition.Column = 0;

            _haveReduction = false;

            _expectedSymbols.Clear();
            _inputTokens.Clear();
            _stack.Clear();
            _lookaheadBuffer = "";

            //==== V4
            _groupStack.Clear();
        }

        /// <summary>
        ///     This function implements the DFA for th parser's lexer.
        ///     It generates a token which is used by the LALR state
        ///     machine.
        /// </summary>
        /// <returns></returns>
        private Token LookaheadDFA()
        {
            var result = new Token();

            //===================================================
            //Match DFA token
            //===================================================

            bool done = false;
            int currentDfa = _dfa.InitialState;
            int currentPosition = 1;
            //Next byte in the input Stream
            int lastAcceptState = -1;
            //We have not yet accepted a character string
            int lastAcceptPosition = -1;

            string ch = Lookahead(1);
            //NO MORE DATA
            if (!(string.IsNullOrEmpty(ch) || ch[0] == ushort.MaxValue))
            {
                while (!(done))
                {
                    // This code searches all the branches of the current DFA state
                    // for the next character in the input Stream. If found the
                    // target state is returned.

                    ch = Lookahead(currentPosition);
                    //End reached, do not match
                    bool found;
                    int target = 0;
                    if (string.IsNullOrEmpty(ch))
                    {
                        found = false;
                    }
                    else
                    {
                        int n = 0;
                        found = false;
                        while (n < _dfa[currentDfa].Edges.Count && !found)
                        {
                            FAEdge edge = _dfa[currentDfa].Edges[n];

                            //==== Look for character in the Character Set Table
                            if (edge.Characters.Contains(ch[0]))
                            {
                                found = true;
                                target = edge.Target;
                                //.TableIndex
                            }
                            n += 1;
                        }
                    }

                    // This block-if statement checks whether an edge was found from the current state. If so, the state and current
                    // position advance. Otherwise it is time to exit the main loop and report the token found (if there was one). 
                    // If the LastAcceptState is -1, then we never found a match and the Error Token is created. Otherwise, a new 
                    // token is created using the Symbol in the Accept State and all the characters that comprise it.

                    if (found)
                    {
                        // This code checks whether the target state accepts a token.
                        // If so, it sets the appropiate variables so when the
                        // algorithm in done, it can return the proper token and
                        // number of characters.

                        //NOT is very important!
                        if ((_dfa[target].Accept != null))
                        {
                            lastAcceptState = target;
                            lastAcceptPosition = currentPosition;
                        }

                        currentDfa = target;
                        currentPosition += 1;

                        //No edge found
                    }
                    else
                    {
                        done = true;
                        // Lexer cannot recognize symbol
                        if (lastAcceptState == -1)
                        {
                            result.Parent = _symbolTable.GetFirstOfType(SymbolType.Error);
                            result.Data = LookaheadBuffer(1);
                            // Create Token, read characters
                        }
                        else
                        {
                            result.Parent = _dfa[lastAcceptState].Accept;
                            result.Data = LookaheadBuffer(lastAcceptPosition);
                            //Data contains the total number of accept characters
                        }
                    }
                    //DoEvents
                }
            }
            else
            {
                // End of file reached, create End Token
                result.Data = string.Empty;
                result.Parent = _symbolTable.GetFirstOfType(SymbolType.End);
            }

            //===================================================
            //Set the new token's position information
            //===================================================
            //Notice, this is a copy, not a linking of an instance. We don't want the user 
            //to be able to alter the main value indirectly.
            result.Position.Copy(_sysPosition);

            return result;
        }

        /// <summary>
        ///     Consume/Remove the characters from the front of the buffer.
        /// </summary>
        /// <param name="charCount"></param>
        private void ConsumeBuffer(int charCount)
        {
            if (charCount <= _lookaheadBuffer.Length)
            {
                // Count Carriage Returns and increment the internal column and line
                // numbers. This is done for the Developer and is not necessary for the
                // DFA algorithm.
                for (int n = 0; n <= charCount - 1; n++)
                {
                    switch (_lookaheadBuffer[n])
                    {
                        case '\n':
                            _sysPosition.Line += 1;
                            _sysPosition.Column = 0;
                            break;
                        case '\r':
                            break;
                            //Ignore, LF is used to inc line to be UNIX friendly
                        default:
                            _sysPosition.Column += 1;
                            break;
                    }
                }

                _lookaheadBuffer = _lookaheadBuffer.Remove(0, charCount);
            }
        }

        private Token ProduceToken()
        {
            // ** VERSION 5.0 **
            //This function creates a token and also takes into account the current
            //lexing mode of the parser. In particular, it contains the group logic. 
            //
            //A stack is used to track the current "group". This replaces the comment
            //level counter. Also, text is appended to the token on the top of the 
            //stack. This allows the group text to returned in one chunk.

            bool done = false;
            Token result = null;

            while (!done)
            {
                Token read = LookaheadDFA();

                //The logic - to determine if a group should be nested - requires that the top of the stack 
                //and the symbol's linked group need to be looked at. Both of these can be unset. So, this section
                //sets a Boolean and avoids errors. We will use this boolean in the logic chain below. 
                bool nestGroup;
                if (read.Type == SymbolType.GroupStart)
                {
                    nestGroup = _groupStack.Count == 0 || _groupStack.Peek().Group.Nesting.Contains(read.Group.TableIndex);
                }
                else
                {
                    nestGroup = false;
                }

                //=================================
                // Logic chain
                //=================================

                if (nestGroup)
                {
                    ConsumeBuffer(((string) read.Data).Length);
                    _groupStack.Push(read);
                }
                else if (_groupStack.Count == 0)
                {
                    //The token is ready to be analyzed.             
                    ConsumeBuffer(((string) read.Data).Length);
                    result = read;
                    done = true;
                }
                else if ((ReferenceEquals(_groupStack.Peek().Group.End, read.Parent)))
                {
                    //End the current group
                    Token pop = _groupStack.Pop();

                    //=== Ending logic
                    if (pop.Group.Ending == Group.EndingMode.Closed)
                    {
                        pop.Data = (string) pop.Data + read.Data; //Append text
                        ConsumeBuffer(((string) read.Data).Length); //Consume token
                    }

                    //We are out of the group. Return pop'd token (which contains all the group text)
                    if (_groupStack.Count == 0)
                    {
                        pop.Parent = pop.Group.Container; //Change symbol to parent
                        result = pop;
                        done = true;
                    }
                    else
                    {
                        Token top = _groupStack.Peek();
                        // ReSharper disable once RedundantCast
                        top.Data += (string) pop.Data; //Append group text to parent
                    }
                }
                else if (read.Type == SymbolType.End)
                {
                    //EOF always stops the loop. The caller function (Parse) can flag a runaway group error.
                    result = read;
                    done = true;
                }
                else
                {
                    //We are in a group, Append to the Token on the top of the stack.
                    //Take into account the Token group mode  
                    Token top = _groupStack.Peek();

                    if (top.Group.Advance == Group.AdvanceMode.Token)
                    {
                        top.Data = (string) top.Data + read.Data; // Append all text
                        ConsumeBuffer(((string) read.Data).Length);
                    }
                    else
                    {
                        top.Data = (string) top.Data + ((string) read.Data)[0]; // Append one character
                        ConsumeBuffer(1);
                    }
                }
            }

            return result;
        }

        /// <summary>
        ///     Performs a parse action on the input. This method is typically used in a loop until either grammar is accepted or
        ///     an error occurs.
        /// </summary>
        /// <returns></returns>
        public ParseMessage Parse()
        {
            ParseMessage message = default(ParseMessage);

            if (!_areTablesLoaded)
            {
                return ParseMessage.NotLoadedError;
            }

            //===================================
            //Loop until breakable event
            //===================================
            bool done = false;
            while (!done)
            {
                Token read;
                if (_inputTokens.Count == 0)
                {
                    read = ProduceToken();
                    _inputTokens.Push(read);

                    message = ParseMessage.TokenRead;
                    done = true;
                }
                else
                {
                    read = _inputTokens.Peek();
                    _currentPosition.Copy(read.Position);
                    //Update current position

                    //Runaway group
                    if (_groupStack.Count != 0)
                    {
                        message = ParseMessage.GroupError;
                        done = true;
                    }
                    else if (read.Type == SymbolType.Noise)
                    {
                        //Just discard. These were already reported to the user.
                        _inputTokens.Pop();
                    }
                    else if (read.Type == SymbolType.Error)
                    {
                        message = ParseMessage.LexicalError;
                        done = true;

                        //Finally, we can parse the token.
                    }
                    else
                    {
                        ParseResult action = ParseLALR(read); //SAME PROCEDURE AS v1

                        switch (action)
                        {
                            case ParseResult.Accept:
                                message = ParseMessage.Accept;
                                done = true;

                                break;
                            case ParseResult.InternalError:
                                message = ParseMessage.InternalError;
                                done = true;

                                break;
                            case ParseResult.ReduceNormal:
                                message = ParseMessage.Reduction;
                                done = true;

                                break;
                            case ParseResult.Shift:
                                //ParseToken() shifted the token on the front of the Token-Queue. 
                                //It now exists on the Token-Stack and must be eliminated from the queue.
                                _inputTokens.Pop();

                                break;
                            case ParseResult.SyntaxError:
                                message = ParseMessage.SyntaxError;
                                done = true;

                                break;
                        }
                    }
                }
            }

            return message;
        }

        private enum ParseResult
        {
            Accept = 1,
            Shift = 2,
            ReduceNormal = 3,
            ReduceEliminated = 4,
            //Trim
            SyntaxError = 5,
            InternalError = 6
        }
    }
}