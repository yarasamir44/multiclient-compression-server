# PARSER
class Parser:
    def _init_(self, tokens):
        self.tokens = tokens
        self.pos = 0

    def current_token(self):
        return self.tokens[self.pos] if self.pos < len(self.tokens) else ('EOF', None)

    def match(self, expected_type):
        kind, value = self.current_token()
        if kind == expected_type:
            self.pos += 1
            return value
        else:
            raise SyntaxError(f"Expected {expected_type} but got {kind}")

    def parse(self):
        self.statement_list()
        print("✅ Parsing completed successfully!")

    def statement_list(self):
        while self.current_token()[0] != 'EOF':
            self.statement()

    def statement(self):
        kind, value = self.current_token()

        if value == 'plan':  # تعريف متغير
            self.declaration()
        elif kind == 'ID':  # إسناد
            self.assignment()
        elif value == 'timeout':  # طباعة
            self.output_stmt()
        elif value == 'ear':  # إدخال
            self.input_stmt()
        elif value == 'referee':  # if
            self.if_stmt()
        elif value == 'drill':  # while
            self.while_stmt()
        elif value == 'practice':  # for
            self.for_stmt()
        elif value == 'score':  # return
            self.return_stmt()
        else:
            raise SyntaxError(f"Unknown statement starting with '{value}'")

    def declaration(self):
        self.match('KEYWORD')  # plan
        var_name = self.match('ID')
        self.match('ASSIGN')
        self.expression()
        print(f"Declared variable '{var_name}'.")

    def assignment(self):
        var_name = self.match('ID')
        self.match('ASSIGN')
        self.expression()
        print(f"Assigned value to '{var_name}'.")

    def output_stmt(self):
        self.match('KEYWORD')  # timeout
        self.match('LPAREN')
        self.expression()
        self.match('RPAREN')
        print("Parsed print (timeout) statement.")

    def input_stmt(self):
        self.match('KEYWORD')  # ear
        self.match('LPAREN')
        self.match('ID')
        self.match('RPAREN')
        print("Parsed input (ear) statement.")

    def if_stmt(self):
        self.match('KEYWORD')  # referee
        self.match('LPAREN')
        self.condition()
        self.match('RPAREN')
        self.statement_list()
        if self.current_token()[1] == 'flag':  # else
            self.match('KEYWORD')
            self.statement_list()
        print("Parsed if-else (referee-flag) statement.")

    def while_stmt(self):
        self.match('KEYWORD')  # drill
        self.match('LPAREN')
        self.condition()
        self.match('RPAREN')
        self.statement_list()
        print("Parsed while (drill) loop.")

    def for_stmt(self):
        self.match('KEYWORD')  # practice
        self.match('LPAREN')
        self.declaration()
        self.condition()
        self.assignment()
        self.match('RPAREN')
        self.statement_list()
        print("Parsed for (practice) loop.")

    def return_stmt(self):
        self.match('KEYWORD')  # score
        self.expression()
        print("Parsed return (score) statement.")

    def condition(self):
        self.factor()
        self.match('OP')  # assuming OP includes comparison ops
        self.expression()

    def expression(self):
        self.term()
        while self.current_token()[1] in ('+', '-'):
            self.match('OP')
            self.term()

    def term(self):
        self.factor()
        while self.current_token()[1] in ('*', '/', '%'):
            self.match('OP')
            self.factor()

    def factor(self):
        kind, value = self.current_token()
        if kind == 'NUMBER':
            self.match('NUMBER')
        elif kind == 'ID':
            self.match('ID')
        elif kind == 'STRING':
            self.match('STRING')
        elif kind == 'LPAREN':
            self.match('LPAREN')
            self.expression()
            self.match('RPAREN')
        else:
            raise SyntaxError(f"Unexpected token in factor: {kind}")
