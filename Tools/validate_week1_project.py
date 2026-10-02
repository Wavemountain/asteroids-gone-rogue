#!/usr/bin/env python3
"""Structural checks for the Asteroids gone rogue Week 1 Unity project."""

from __future__ import annotations

import hashlib
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
TITLE = "Asteroids gone rogue"
ERRORS: list[str] = []


def guid_for(relative: str) -> str:
    return hashlib.md5(f"asteroids-gone-rogue:{relative}".encode("utf-8")).hexdigest()


def err(message: str) -> None:
    ERRORS.append(message)


def require(path: Path, hint: str = "") -> None:
    if not path.exists():
        err(f"missing {path.relative_to(ROOT)}" + (f" ({hint})" if hint else ""))


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8")


_CS_BLOCK_COMMENT = re.compile(r"/\*.*?\*/", re.DOTALL)
_CS_LINE_COMMENT = re.compile(r"//.*?$", re.MULTILINE)
_CS_STRING = re.compile(r'"(?:\\.|[^"\\])*"')
_SHADER_DECL = re.compile(r"\bShader\s+(\w+)\b")
_MATERIAL_CTOR = re.compile(r"\bnew\s+Material\s*\(")


def strip_cs_noise(source: str) -> str:
    """Drop comments and string literals so static C# scans stay structural."""
    cleaned = _CS_BLOCK_COMMENT.sub(" ", source)
    cleaned = _CS_LINE_COMMENT.sub(" ", cleaned)
    return _CS_STRING.sub('""', cleaned)


def _split_cs_statements(source: str) -> list[str]:
    statements: list[str] = []
    buf: list[str] = []
    depth = 0
    for ch in source:
        if ch in "([{":
            depth += 1
        elif ch in ")]}":
            depth = max(0, depth - 1)
        if ch == ";" and depth == 0:
            stmt = "".join(buf).strip()
            if stmt:
                statements.append(stmt)
            buf = []
            continue
        buf.append(ch)
    tail = "".join(buf).strip()
    if tail:
        statements.append(tail)
    return statements


def _paren_args(source: str, open_index: int) -> str | None:
    if open_index < 0 or open_index >= len(source) or source[open_index] != "(":
        return None
    depth = 0
    for i in range(open_index, len(source)):
        ch = source[i]
        if ch == "(":
            depth += 1
        elif ch == ")":
            depth -= 1
            if depth == 0:
                return source[open_index + 1 : i]
    return None


def _has_toplevel_ternary(expr: str) -> bool:
    """True when `?:` is the top-level operator (Unity 6000.6 CS1503 / target-typed ?:)."""
    depth = 0
    i = 0
    while i < len(expr):
        ch = expr[i]
        if ch in "([{":
            depth += 1
        elif ch in ")]}":
            depth = max(0, depth - 1)
        elif ch == "?" and depth == 0:
            nxt = expr[i + 1] if i + 1 < len(expr) else ""
            if nxt not in ".?":
                return True
        i += 1
    return False


def shader_conditional_violations(source: str) -> list[str]:
    """Flag ternary→Shader and target-typed `?:` passed to Material(Shader) (CS1503)."""
    hits: list[str] = []
    cleaned = strip_cs_noise(source)
    shader_names: set[str] = set()
    for stmt in _split_cs_statements(cleaned):
        for match in _SHADER_DECL.finditer(stmt):
            shader_names.add(match.group(1))

        decl = re.search(r"\bShader\s+(\w+)\s*=\s*(.*)$", stmt, re.DOTALL)
        if decl and _has_toplevel_ternary(decl.group(2)):
            hits.append(f"ternary assigned to Shader {decl.group(1)}")

        assign = re.search(r"^(\w+)\s*=\s*(.*)$", stmt, re.DOTALL)
        if assign and assign.group(1) in shader_names and _has_toplevel_ternary(assign.group(2)):
            label = f"ternary assigned to Shader {assign.group(1)}"
            if label not in hits:
                hits.append(label)

        search_from = 0
        while True:
            ctor = _MATERIAL_CTOR.search(stmt, search_from)
            if not ctor:
                break
            args = _paren_args(stmt, ctor.end() - 1)
            if args is not None and _has_toplevel_ternary(args):
                hits.append("target-typed ternary passed to new Material")
            search_from = ctor.end()
    return hits


LFS_POINTER_PREFIX = b"version https://git-lfs.github.com/spec/v1"


def is_lfs_pointer(path: Path) -> bool:
    """True when the working-tree file is Git LFS pointer text, not an FBX binary."""
    if not path.is_file():
        return False
    if path.stat().st_size < 1000:
        return True
    head = path.read_bytes()[:64]
    return head.startswith(LFS_POINTER_PREFIX)


# Unity's C# compiler (CS0136) rejects a local in a nested block when the same
# name is declared anywhere in an enclosing block of that method — including
# textually later. CS0128 is the same name twice in one block. Sibling blocks
# may reuse a name. This is a heuristic scanner, not a full C# parser.
_CS_MODIFIERS = frozenset(
    {
        "public",
        "private",
        "protected",
        "internal",
        "static",
        "sealed",
        "override",
        "virtual",
        "abstract",
        "async",
        "new",
        "partial",
        "readonly",
        "volatile",
        "extern",
        "unsafe",
        "required",
        "file",
        "ref",
    }
)
_CS_NON_TYPE = frozenset(
    {
        "if",
        "else",
        "for",
        "foreach",
        "while",
        "do",
        "switch",
        "case",
        "default",
        "try",
        "catch",
        "finally",
        "lock",
        "using",
        "return",
        "throw",
        "break",
        "continue",
        "goto",
        "new",
        "sizeof",
        "typeof",
        "checked",
        "unchecked",
        "fixed",
        "unsafe",
        "const",
        "out",
        "ref",
        "in",
        "params",
        "this",
        "base",
        "null",
        "true",
        "false",
        "get",
        "set",
        "add",
        "remove",
        "init",
        "class",
        "struct",
        "enum",
        "interface",
        "namespace",
        "operator",
        "event",
        "public",
        "private",
        "protected",
        "internal",
        "static",
        "sealed",
        "override",
        "virtual",
        "abstract",
        "async",
        "partial",
        "readonly",
        "volatile",
        "extern",
        "yield",
        "when",
        "where",
        "nameof",
        "delegate",
        "implicit",
        "explicit",
        "await",
        "is",
        "as",
        "and",
        "or",
        "not",
        "with",
        "record",
        "stackalloc",
        "file",
        "required",
    }
)
_CS_ACCESSOR = frozenset({"get", "set", "init", "add", "remove"})
_CS_TOKEN = re.compile(
    r"@?[A-Za-z_][A-Za-z0-9_]*"
    r"|==|!=|<=|>=|\+\+|--|&&|\|\||=>|\+=|-=|\*=|/=|%=|\?\?|\?\."
    r"|0[xX][0-9A-Fa-f]+"
    r"|\d+\.\d+[fFdDmM]?|\d+[fFdDlLuU]*"
    r"|[{}()\[\];,.=<>+\-*/%&|^!~?:]"
)


def _strip_cs_preserve_lines(source: str) -> str:
    """Replace comments, strings, and preprocessor lines with spaces. Keep newlines."""
    out: list[str] = []
    i = 0
    n = len(source)
    while i < n:
        c = source[i]
        nxt = source[i + 1] if i + 1 < n else ""
        if c == "/" and nxt == "/":
            while i < n and source[i] != "\n":
                out.append(" ")
                i += 1
            continue
        if c == "/" and nxt == "*":
            out.append(" ")
            out.append(" ")
            i += 2
            while i < n and not (source[i] == "*" and i + 1 < n and source[i + 1] == "/"):
                out.append("\n" if source[i] == "\n" else " ")
                i += 1
            if i < n:
                out.append(" ")
                out.append(" ")
                i += 2
            continue
        if c == '"':
            out.append(" ")
            i += 1
            while i < n and source[i] != '"':
                if source[i] == "\\" and i + 1 < n:
                    out.append(" ")
                    out.append("\n" if source[i + 1] == "\n" else " ")
                    i += 2
                    continue
                out.append("\n" if source[i] == "\n" else " ")
                i += 1
            if i < n:
                out.append(" ")
                i += 1
            continue
        if c == "'":
            out.append(" ")
            i += 1
            while i < n and source[i] != "'":
                if source[i] == "\\" and i + 1 < n:
                    out.append(" ")
                    out.append(" ")
                    i += 2
                    continue
                out.append("\n" if source[i] == "\n" else " ")
                i += 1
            if i < n:
                out.append(" ")
                i += 1
            continue
        out.append(c)
        i += 1
    text = "".join(out)
    lines = text.split("\n")
    blanked = [" " * len(line) if line.lstrip().startswith("#") else line for line in lines]
    return "\n".join(blanked)


def _tokenize_cs(source: str) -> list[tuple[str, int]]:
    tokens: list[tuple[str, int]] = []
    i = 0
    line = 1
    n = len(source)
    while i < n:
        ch = source[i]
        if ch.isspace():
            if ch == "\n":
                line += 1
            i += 1
            continue
        match = _CS_TOKEN.match(source, i)
        if not match:
            i += 1
            continue
        tokens.append((match.group(0), line))
        i = match.end()
    return tokens


class _CsScope:
    def __init__(self, parent: "_CsScope | None", method: str) -> None:
        self.parent = parent
        self.method = method
        self.locals: list[tuple[str, int]] = []
        self.children: list[_CsScope] = []

    def add(self, name: str, line: int) -> None:
        if name and name != "_":
            self.locals.append((name, line))


class _CsShadowScanner:
    """Per-method local declaration spaces for CS0136 / CS0128."""

    def __init__(self, tokens: list[tuple[str, int]]) -> None:
        self.toks = tokens
        self.i = 0
        self.roots: list[_CsScope] = []
        self.warnings: list[str] = []

    def peek(self, k: int = 0) -> str:
        j = self.i + k
        if j < 0 or j >= len(self.toks):
            return ""
        return self.toks[j][0]

    def line(self, k: int = 0) -> int:
        j = self.i + k
        if j < 0 or j >= len(self.toks):
            return 1
        return self.toks[j][1]

    def eof(self) -> bool:
        return self.i >= len(self.toks)

    def check(self, text: str) -> bool:
        return self.peek() == text

    def advance(self) -> str:
        if self.eof():
            return ""
        text = self.peek()
        self.i += 1
        return text

    def expect(self, text: str) -> bool:
        if self.check(text):
            self.advance()
            return True
        self.warnings.append(f"line {self.line()}: expected {text!r} but found {self.peek()!r}")
        return False

    def is_ident(self, k: int = 0) -> bool:
        text = self.peek(k)
        if not text:
            return False
        if text[0] == "@":
            text = text[1:]
        return bool(text) and (text[0].isalpha() or text[0] == "_")

    def ident_text(self, raw: str | None = None) -> str:
        text = self.peek() if raw is None else raw
        return text[1:] if text.startswith("@") else text

    def can_start_type(self, k: int = 0) -> bool:
        if not self.is_ident(k):
            return False
        return self.ident_text(self.peek(k)) not in _CS_NON_TYPE

    def skip_modifiers(self) -> None:
        while self.ident_text(self.peek()) in _CS_MODIFIERS:
            self.advance()

    def skip_attributes(self) -> None:
        while self.check("["):
            depth = 0
            while not self.eof():
                if self.check("["):
                    depth += 1
                elif self.check("]"):
                    depth -= 1
                    self.advance()
                    if depth == 0:
                        break
                    continue
                self.advance()

    def skip_balanced(self, open_tok: str, close_tok: str) -> None:
        if not self.expect(open_tok):
            return
        depth = 1
        while not self.eof() and depth:
            if self.check(open_tok):
                depth += 1
            elif self.check(close_tok):
                depth -= 1
            self.advance()

    def skip_generic_args(self) -> None:
        if not self.expect("<"):
            return
        depth = 1
        while not self.eof() and depth:
            if self.check("<"):
                depth += 1
            elif self.check(">"):
                depth -= 1
            self.advance()

    def is_array_rank(self) -> bool:
        if not self.check("["):
            return False
        j = 1
        while self.peek(j) == ",":
            j += 1
        return self.peek(j) == "]"

    def consume_array_rank(self) -> None:
        self.advance()  # [
        while self.check(","):
            self.advance()
        self.expect("]")

    def parse_type(self) -> bool:
        if not self.can_start_type():
            return False
        self.advance()
        while self.check("."):
            if not self.is_ident(1):
                break
            self.advance()
            self.advance()
        if self.check("<"):
            self.skip_generic_args()
        while True:
            if self.check("?"):
                self.advance()
                continue
            if self.is_array_rank():
                self.consume_array_rank()
                continue
            break
        return True

    def generic_end(self) -> int | None:
        """Index just past a type-argument list, or None when `<` is a comparison."""
        if not self.check("<"):
            return None
        j = self.i + 1
        depth = 1
        n = len(self.toks)
        allowed = {",", ".", "?", "[", "]"}
        while j < n and depth > 0:
            text = self.toks[j][0]
            bare = text[1:] if text.startswith("@") else text
            if text == "<":
                depth += 1
            elif text == ">":
                depth -= 1
            elif text in allowed or bare[:1].isalpha() or bare[:1] == "_":
                pass
            else:
                return None
            j += 1
        if depth == 0:
            return j
        return None

    def _stop_expr(self, paren: int, brack: int, brace: int, stop_comma: bool, stop_colon: bool) -> bool:
        if paren or brack or brace:
            return False
        text = self.peek()
        if text in {";", ")", "]", "}"}:
            return True
        if stop_comma and text == ",":
            return True
        if stop_colon and text == ":":
            return True
        return False

    def skip_expression(self, scope: _CsScope | None, stop_comma: bool = False, stop_colon: bool = False) -> None:
        paren = brack = brace = 0
        while not self.eof():
            start = self.i
            if paren == brack == brace == 0 and self._at_lambda():
                self.parse_lambda(scope)
                if self.i == start:
                    self.advance()
                continue
            if self.check("out") and self.try_inline_out(scope):
                continue
            if self.check("is") and self.try_is_pattern(scope):
                continue
            if self._stop_expr(paren, brack, brace, stop_comma, stop_colon):
                return
            if self.check("<"):
                end = self.generic_end()
                if end is not None:
                    self.i = end
                    continue
            text = self.peek()
            if text == "(":
                paren += 1
            elif text == ")":
                if paren == 0:
                    return
                paren -= 1
            elif text == "[":
                brack += 1
            elif text == "]":
                if brack == 0:
                    return
                brack -= 1
            elif text == "{":
                brace += 1
            elif text == "}":
                if brace == 0:
                    return
                brace -= 1
            self.advance()
            if self.i == start:
                self.advance()

    def _at_lambda(self) -> bool:
        if self.is_ident() and self.ident_text() not in _CS_NON_TYPE and self.peek(1) == "=>":
            return True
        if not self.check("("):
            return False
        depth = 0
        j = self.i
        n = len(self.toks)
        while j < n:
            text = self.toks[j][0]
            if text == "(":
                depth += 1
            elif text == ")":
                depth -= 1
                if depth == 0:
                    return j + 1 < n and self.toks[j + 1][0] == "=>"
            j += 1
        return False

    def parse_lambda(self, scope: _CsScope | None) -> None:
        method = scope.method if scope is not None else "<lambda>"
        lam = _CsScope(scope, method)
        if scope is not None:
            scope.children.append(lam)
        else:
            self.roots.append(lam)
        if self.check("("):
            self.advance()
            if not self.check(")"):
                while not self.eof() and not self.check(")"):
                    if self._looks_like_typed_param():
                        self.parse_type()
                    if self.is_ident():
                        lam.add(self.ident_text(), self.line())
                        self.advance()
                    if self.check(","):
                        self.advance()
                        continue
                    break
            self.expect(")")
        elif self.is_ident():
            lam.add(self.ident_text(), self.line())
            self.advance()
        self.expect("=>")
        if self.check("{"):
            body = _CsScope(lam, method)
            lam.children.append(body)
            self.parse_block(body)
        else:
            self.skip_expression(lam, stop_comma=True)

    def try_inline_out(self, scope: _CsScope | None) -> bool:
        """Consume `out Type name` / `out var name`. Leave bare `out name` alone."""
        if not self.check("out"):
            return False
        if self.peek(1) == "var" and self.is_ident(2) and self.peek(3) in {",", ")", ";"}:
            if scope is not None:
                scope.add(self.ident_text(self.peek(2)), self.line(2))
            self.i += 3
            return True
        saved = self.i
        self.advance()
        if not self.parse_type() or not self.is_ident() or self.peek(1) not in {",", ")", ";"}:
            self.i = saved
            return False
        if scope is not None:
            scope.add(self.ident_text(), self.line())
        self.advance()
        return True

    def try_is_pattern(self, scope: _CsScope | None) -> bool:
        if not self.check("is"):
            return False
        saved = self.i
        self.advance()
        if not self.parse_type() or not self.is_ident():
            self.i = saved
            return False
        nxt = self.peek(1)
        if nxt not in {")", ";", ",", "&&", "||", "?", "{", ":"}:
            self.i = saved
            return False
        if scope is not None:
            scope.add(self.ident_text(), self.line())
        self.advance()
        return True

    def _looks_like_typed_param(self) -> bool:
        saved = self.i
        if not self.parse_type() or not self.is_ident():
            self.i = saved
            return False
        self.advance()
        ok = self.peek() in {",", ")", "="}
        self.i = saved
        return ok

    def looks_like_local_decl(self) -> bool:
        saved = self.i
        if self.check("const"):
            self.advance()
        if not self.parse_type() or not self.is_ident():
            self.i = saved
            return False
        self.i = saved
        return True

    def parse_declarators(self, scope: _CsScope | None, first_consumed: bool = False) -> None:
        if not first_consumed:
            if not self.is_ident():
                return
            if scope is not None:
                scope.add(self.ident_text(), self.line())
            self.advance()
        if self.check("="):
            self.advance()
            self.skip_expression(scope, stop_comma=True)
        while self.check(","):
            self.advance()
            if not self.is_ident():
                break
            if scope is not None:
                scope.add(self.ident_text(), self.line())
            self.advance()
            if self.check("="):
                self.advance()
                self.skip_expression(scope, stop_comma=True)

    def try_parse_local(self, scope: _CsScope) -> bool:
        saved = self.i
        if self.check("const"):
            self.advance()
        if not self.parse_type() or not self.is_ident():
            self.i = saved
            return False
        name = self.ident_text()
        name_line = self.line()
        self.advance()
        if self.check("("):
            params = self.parse_param_list()
            self.skip_where()
            scope.add(name, name_line)
            self.finish_method(name, params, scope)
            return True
        scope.add(name, name_line)
        self.parse_declarators(scope, first_consumed=True)
        if self.check(";"):
            self.advance()
        return True

    def parse_param_list(self) -> list[tuple[str, int]]:
        params: list[tuple[str, int]] = []
        if not self.expect("("):
            return params
        while not self.eof() and not self.check(")"):
            self.skip_attributes()
            while self.ident_text() in {"ref", "out", "in", "params", "this"}:
                self.advance()
            if self.check(")"):
                break
            if not self.parse_type():
                if self.check(","):
                    self.advance()
                    continue
                break
            if self.is_ident():
                params.append((self.ident_text(), self.line()))
                self.advance()
            if self.check("="):
                self.advance()
                self.skip_expression(None, stop_comma=True)
            if self.check(","):
                self.advance()
                continue
            break
        self.expect(")")
        return params

    def skip_where(self) -> None:
        while self.check("where"):
            while not self.eof() and not self.check("{") and not self.check(";") and not self.check("=>"):
                self.advance()

    def skip_ctor_init(self) -> None:
        if not self.check(":"):
            return
        self.advance()
        if self.is_ident():
            self.advance()
        if self.check("("):
            self.skip_balanced("(", ")")

    def finish_method(self, name: str, params: list[tuple[str, int]], parent: _CsScope | None) -> None:
        self.skip_where()
        self.skip_ctor_init()
        if self.check(";"):
            self.advance()
            return
        param_scope = _CsScope(parent, name)
        for pname, pline in params:
            param_scope.add(pname, pline)
        if parent is not None:
            parent.children.append(param_scope)
        else:
            self.roots.append(param_scope)
        if self.check("=>"):
            self.advance()
            self.skip_expression(param_scope)
            self.expect(";")
            return
        if self.check("{"):
            body = _CsScope(param_scope, name)
            param_scope.children.append(body)
            self.parse_block(body)
            return
        self.warnings.append(f"line {self.line()}: method {name} has no body")

    def parse_block(self, scope: _CsScope) -> None:
        if not self.expect("{"):
            return
        guard = 0
        limit = len(self.toks) + 2
        while not self.eof() and not self.check("}"):
            before = self.i
            self.parse_statement(scope)
            if self.i == before:
                self.advance()
            guard += 1
            if guard > limit:
                self.warnings.append(f"line {self.line()}: stopped parsing block in {scope.method}")
                break
        self.expect("}")

    def parse_embedded(self, scope: _CsScope) -> None:
        if self.check("{"):
            child = _CsScope(scope, scope.method)
            scope.children.append(child)
            self.parse_block(child)
        else:
            self.parse_statement(scope)

    def parse_statement(self, scope: _CsScope) -> None:
        if self.eof() or self.check("}"):
            return
        if self.check(";"):
            self.advance()
            return
        if self.check("{"):
            self.parse_embedded(scope)
            return
        text = self.ident_text()
        if text == "if":
            self.parse_if(scope)
            return
        if text == "for":
            self.parse_for(scope)
            return
        if text == "foreach":
            self.parse_foreach(scope)
            return
        if text == "while":
            self.advance()
            self.expect("(")
            self.skip_expression(scope)
            self.expect(")")
            self.parse_embedded(scope)
            return
        if text == "do":
            self.advance()
            self.parse_embedded(scope)
            if self.check("while"):
                self.advance()
                self.expect("(")
                self.skip_expression(scope)
                self.expect(")")
            self.expect(";")
            return
        if text == "switch":
            self.parse_switch(scope)
            return
        if text == "try":
            self.parse_try(scope)
            return
        if text == "lock":
            self.advance()
            self.expect("(")
            self.skip_expression(scope)
            self.expect(")")
            self.parse_embedded(scope)
            return
        if text == "using":
            self.parse_using_stmt(scope)
            return
        if text in {"return", "throw"}:
            self.advance()
            if not self.check(";"):
                self.skip_expression(scope)
            self.expect(";")
            return
        if text in {"break", "continue"}:
            self.advance()
            self.expect(";")
            return
        if text == "case":
            self.advance()
            self.skip_expression(scope, stop_colon=True)
            self.expect(":")
            return
        if text == "default":
            self.advance()
            if self.check(":"):
                self.advance()
            elif self.check("("):
                self.skip_expression(scope)
                self.expect(";")
            return
        if text in {"checked", "unchecked", "unsafe", "fixed"}:
            self.advance()
            if self.check("("):
                self.skip_balanced("(", ")")
            if self.check("{"):
                self.parse_embedded(scope)
            else:
                self.expect(";")
            return
        if self.is_ident() and self.peek(1) == ":" and text not in _CS_NON_TYPE:
            self.advance()
            self.advance()
            return
        if self.try_parse_local(scope):
            return
        self.skip_expression(scope)
        if self.check(";"):
            self.advance()

    def parse_if(self, scope: _CsScope) -> None:
        self.advance()
        self.expect("(")
        self.skip_expression(scope)
        self.expect(")")
        self.parse_embedded(scope)
        if self.ident_text() == "else":
            self.advance()
            self.parse_embedded(scope)

    def parse_for(self, scope: _CsScope) -> None:
        self.advance()
        self.expect("(")
        for_scope = _CsScope(scope, scope.method)
        scope.children.append(for_scope)
        if self.check(";"):
            self.advance()
        elif self.looks_like_local_decl():
            self.try_parse_local(for_scope)
        else:
            self.skip_expression(for_scope, stop_comma=False)
            self.expect(";")
        if not self.check(";"):
            self.skip_expression(for_scope)
        self.expect(";")
        if not self.check(")"):
            self.skip_expression(for_scope)
        self.expect(")")
        self.parse_embedded(for_scope)

    def parse_foreach(self, scope: _CsScope) -> None:
        self.advance()
        self.expect("(")
        each = _CsScope(scope, scope.method)
        scope.children.append(each)
        if self.parse_type() and self.is_ident():
            each.add(self.ident_text(), self.line())
            self.advance()
        if self.ident_text() == "in":
            self.advance()
        self.skip_expression(each)
        self.expect(")")
        self.parse_embedded(each)

    def parse_switch(self, scope: _CsScope) -> None:
        self.advance()
        self.expect("(")
        self.skip_expression(scope)
        self.expect(")")
        if not self.expect("{"):
            return
        sw = _CsScope(scope, scope.method)
        scope.children.append(sw)
        guard = 0
        limit = len(self.toks) + 2
        while not self.eof() and not self.check("}"):
            before = self.i
            if self.ident_text() == "case":
                self.advance()
                self.skip_expression(sw, stop_colon=True)
                self.expect(":")
            elif self.ident_text() == "default" and self.peek(1) == ":":
                self.advance()
                self.advance()
            else:
                self.parse_statement(sw)
            if self.i == before:
                self.advance()
            guard += 1
            if guard > limit:
                break
        self.expect("}")

    def parse_try(self, scope: _CsScope) -> None:
        self.advance()
        self.parse_embedded(scope)
        while self.ident_text() == "catch":
            self.advance()
            caught = _CsScope(scope, scope.method)
            scope.children.append(caught)
            if self.check("("):
                self.advance()
                if not self.check(")"):
                    self.parse_type()
                    if self.is_ident() and self.peek(1) in {")", "when"}:
                        caught.add(self.ident_text(), self.line())
                        self.advance()
                    if self.ident_text() == "when":
                        self.advance()
                        self.expect("(")
                        self.skip_expression(caught)
                        self.expect(")")
                self.expect(")")
            self.parse_embedded(caught)
        if self.ident_text() == "finally":
            self.advance()
            self.parse_embedded(scope)

    def parse_using_stmt(self, scope: _CsScope) -> None:
        self.advance()
        if self.check("("):
            self.advance()
            used = _CsScope(scope, scope.method)
            scope.children.append(used)
            if self.looks_like_local_decl():
                saved = self.i
                if self.check("const"):
                    self.advance()
                self.parse_type()
                self.parse_declarators(used)
                if self.i == saved:
                    self.skip_expression(used)
            else:
                self.skip_expression(used)
            self.expect(")")
            self.parse_embedded(used)
            return
        self.try_parse_local(scope)

    def parse_enum(self) -> None:
        self.expect("enum")
        if self.is_ident():
            self.advance()
        if self.check(":"):
            self.advance()
            self.parse_type()
        if self.check("{"):
            self.skip_balanced("{", "}")

    def parse_class(self) -> None:
        self.advance()  # class / struct / interface / record
        name = self.ident_text() if self.is_ident() else ""
        if self.is_ident():
            self.advance()
        if self.check("<"):
            self.skip_generic_args()
        while not self.eof() and not self.check("{"):
            self.advance()
        if not self.expect("{"):
            return
        guard = 0
        limit = len(self.toks) + 2
        while not self.eof() and not self.check("}"):
            before = self.i
            self.parse_member(name)
            if self.i == before:
                self.advance()
            guard += 1
            if guard > limit:
                self.warnings.append(f"line {self.line()}: stopped parsing type {name}")
                break
        self.expect("}")

    def parse_member(self, class_name: str) -> None:
        self.skip_attributes()
        self.skip_modifiers()
        if self.eof() or self.check("}"):
            return
        kind = self.ident_text()
        if kind in {"class", "struct", "interface", "record"}:
            self.parse_class()
            return
        if kind == "enum":
            self.parse_enum()
            return
        if kind == "~":
            self.advance()
            if self.is_ident():
                self.advance()
            params = self.parse_param_list()
            self.finish_method(class_name, params, None)
            return
        if kind == "const":
            self.advance()
            if self.parse_type():
                self.parse_declarators(None)
            self.expect(";")
            return
        if kind == "event":
            self.advance()
            if self.parse_type() and self.is_ident():
                self.advance()
            if self.check("{"):
                self.parse_accessors("event")
            else:
                if self.check("="):
                    self.advance()
                    self.skip_expression(None)
                self.expect(";")
            return
        if self.is_ident() and self.ident_text() not in _CS_NON_TYPE and self.peek(1) == "(":
            ctor = self.ident_text()
            self.advance()
            params = self.parse_param_list()
            self.finish_method(ctor or class_name, params, None)
            return
        if not self.parse_type():
            self.advance()
            return
        if not self.is_ident():
            if self.check(";"):
                self.advance()
            return
        name = self.ident_text()
        self.advance()
        if self.check("("):
            params = self.parse_param_list()
            self.finish_method(name, params, None)
            return
        if self.check("{"):
            self.parse_accessors(name)
            return
        if self.check("=>"):
            self.advance()
            scope = _CsScope(None, name)
            self.roots.append(scope)
            self.skip_expression(scope)
            self.expect(";")
            return
        self.parse_declarators(None, first_consumed=True)
        self.expect(";")

    def parse_accessors(self, prop_name: str) -> None:
        if not self.expect("{"):
            return
        while not self.eof() and not self.check("}"):
            self.skip_attributes()
            self.skip_modifiers()
            acc = self.ident_text()
            if acc not in _CS_ACCESSOR:
                if self.check("}"):
                    break
                self.advance()
                continue
            acc_line = self.line()
            self.advance()
            params = [("value", acc_line)] if acc in {"set", "init", "add"} else []
            if self.check(";"):
                self.advance()
            elif self.check("=>"):
                self.advance()
                scope = _CsScope(None, f"{prop_name}.{acc}")
                for pname, pline in params:
                    scope.add(pname, pline)
                self.roots.append(scope)
                self.skip_expression(scope)
                self.expect(";")
            elif self.check("{"):
                self.finish_method(f"{prop_name}.{acc}", params, None)
            else:
                self.advance()
        self.expect("}")
        if self.check("="):
            self.advance()
            scope = _CsScope(None, prop_name)
            self.roots.append(scope)
            self.skip_expression(scope)
            self.expect(";")

    def starts_type_decl(self) -> bool:
        saved = self.i
        self.skip_attributes()
        self.skip_modifiers()
        ok = self.ident_text() in {"class", "struct", "enum", "interface", "record"}
        self.i = saved
        return ok

    def parse_type_decl(self) -> None:
        self.skip_attributes()
        self.skip_modifiers()
        if self.ident_text() == "enum":
            self.parse_enum()
        elif self.ident_text() in {"class", "struct", "interface", "record"}:
            self.parse_class()
        else:
            self.advance()

    def parse_namespace(self) -> None:
        self.expect("namespace")
        while self.is_ident() or self.check("."):
            self.advance()
        if self.check(";"):
            self.advance()
            self.parse_namespace_members(until_brace=False)
            return
        if not self.expect("{"):
            return
        self.parse_namespace_members(until_brace=True)
        self.expect("}")

    def parse_namespace_members(self, until_brace: bool) -> None:
        guard = 0
        limit = len(self.toks) + 2
        while not self.eof() and not (until_brace and self.check("}")):
            before = self.i
            self.skip_attributes()
            if self.check("namespace"):
                self.parse_namespace()
            elif self.starts_type_decl():
                self.parse_type_decl()
            else:
                self.advance()
            if self.i == before:
                self.advance()
            guard += 1
            if guard > limit:
                break

    def parse_file(self) -> None:
        while not self.eof() and self.ident_text() in {"using", "extern"}:
            while not self.eof() and not self.check(";"):
                self.advance()
            self.expect(";")
        guard = 0
        limit = len(self.toks) + 2
        while not self.eof():
            before = self.i
            self.skip_attributes()
            if self.check("namespace"):
                self.parse_namespace()
            elif self.starts_type_decl():
                self.parse_type_decl()
            elif self.looks_like_local_decl() or (
                self.is_ident() and self.ident_text() not in _CS_NON_TYPE and self.peek(1) == "("
            ):
                # Snippet: a method (or a class body) without a wrapping type.
                self.parse_member("")
            else:
                self.advance()
            if self.i == before:
                self.advance()
            guard += 1
            if guard > limit:
                self.warnings.append("stopped parsing file")
                break

    def violations(self) -> list[str]:
        hits: list[str] = []

        def walk(scope: _CsScope, enclosing: dict[str, int]) -> None:
            own: dict[str, int] = {}
            for name, line in scope.locals:
                if name in own:
                    hits.append(
                        f"{scope.method}: duplicate local '{name}' in the same block "
                        f"(lines {own[name]} and {line}, CS0128)"
                    )
                else:
                    own[name] = line
                if name in enclosing:
                    hits.append(
                        f"{scope.method}: local '{name}' at line {line} is declared in a nested block "
                        f"and shadows '{name}' at line {enclosing[name]} in an enclosing block (CS0136)"
                    )
            merged = dict(enclosing)
            merged.update(own)
            for child in scope.children:
                walk(child, merged)

        for root in self.roots:
            walk(root, {})
        return hits


def local_shadow_violations(source: str) -> list[str]:
    """CS0136/CS0128 locals: nested block vs enclosing block (any position), or duplicates.

    A local declared in a nested block conflicts with the same name declared anywhere
    in an enclosing block of that method, including a declaration that appears later.
    Sibling blocks may reuse a name. Method parameters count as the outermost scope.
    """
    cleaned = _strip_cs_preserve_lines(source)
    scanner = _CsShadowScanner(_tokenize_cs(cleaned))
    scanner.parse_file()
    return scanner.violations()


def _dict_keys(block: str) -> set[str]:
    return set(re.findall(r'\{\s*"([^"]+)"\s*,', block))


def _enum_names(source: str, enum_name: str) -> list[str]:
    body = source.split(f"enum {enum_name}")[1].split("}")[0]
    return re.findall(r"^\s*([A-Z][A-Za-z0-9_]*)\s*,?\s*$", body, re.M)


def check_loc_key_parity(loc_src: str) -> None:
    """Every EN loc key exists in SV and every SV key exists in EN.

    EN is the literal Loc.T/Tf keys, dynamic shop/enemy/mode keys, and the
    English dictionary for table-only keys. SV is the Swedish dictionary.
    """
    parts = loc_src.split("private static readonly Dictionary")
    if len(parts) < 3:
        err("Loc must define a Swedish dictionary and an English dictionary")
        return

    swedish = _dict_keys(parts[1].split("};")[0])
    english = _dict_keys(parts[2].split("};")[0])
    literals: set[str] = set()
    for path in (ROOT / "Assets/Scripts").rglob("*.cs"):
        text = path.read_text(encoding="utf-8")
        for key in re.findall(r'Loc\.Tf?\(\s*"([^"]+)"', text):
            if key.endswith("."):
                continue
            literals.add(key)

    dynamic: set[str] = set()
    shop = read(ROOT / "Assets/Scripts/Core/ShopCatalog.cs")
    for upgrade in re.findall(r"UpgradeId\.(\w+)", shop):
        dynamic.add(f"shop.title.{upgrade}")
        dynamic.add(f"shop.desc.{upgrade}")
    for name in _enum_names(read(ROOT / "Assets/Scripts/Combat/EnemyKind.cs"), "EnemyKind"):
        dynamic.add(f"enemy.{name}")
    for name in _enum_names(read(ROOT / "Assets/Scripts/Player/FireMode.cs"), "FireMode"):
        dynamic.add(f"mode.{name}")

    en_keys = literals | dynamic | english
    missing_sv = sorted(en_keys - swedish)
    missing_en = sorted(swedish - en_keys)
    if missing_sv:
        err("SV loc table missing keys: " + ", ".join(missing_sv[:12]))
    if missing_en:
        err("EN loc table missing keys: " + ", ".join(missing_en[:12]))
    if "credits.body" not in swedish or "credits.body" not in en_keys:
        err("credits.body must exist in both the EN and SV loc tables")
        return

    sv_credits = parts[1].split('"credits.body"')[1].split("},")[0]
    for token in ("Ljud", "Typsnitt", "Kenney.nl", "Kenney Future", "Speltest"):
        if token not in sv_credits:
            err(f"Swedish credits.body is missing '{token}'")


def main() -> int:
    require(ROOT / "Packages/manifest.json")
    require(ROOT / "ProjectSettings/ProjectVersion.txt")
    require(ROOT / "ProjectSettings/ProjectSettings.asset")
    require(ROOT / "ProjectSettings/EditorBuildSettings.asset")
    require(ROOT / "ProjectSettings/TagManager.asset")
    require(ROOT / "Assets/Scenes/Play.unity")
    require(ROOT / "Assets/Scripts/Content/GameBootstrap.cs")
    require(ROOT / "Assets/Art/Import/IMPORT.md")
    require(ROOT / "Assets/Art/Import/MANIFEST.md")
    require(ROOT / "README.md")
    require(ROOT / "MERGE_CHECKLIST.md")
    require(ROOT / "Docs/HUB_SMOKE.md")
    require(ROOT / "Docs/StoreCaptures/README.md")
    checklist = read(ROOT / "MERGE_CHECKLIST.md")
    if "Do not merge until Wagge" not in checklist and "until Wagge says yes" not in checklist:
        err("MERGE_CHECKLIST.md must say not to merge until Wagge says yes")
    if "6000.6.0f1" not in checklist:
        err("MERGE_CHECKLIST.md must name Unity 6000.6.0f1")
    require(ROOT / "CREDITS.md")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserSmall_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/click_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/minimize_005.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserRetro_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserLarge_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/impactMetal_003.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/impactMetal_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/impactMetal_001.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/explosionCrunch_001.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/phaserUp5.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/lowThreeTone.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/phaseJump1.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/slime_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/impactMetal_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserSmall_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserSmall_004.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/zap1.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/spaceTrash1.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/spaceTrash3.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/forceField_001.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/lowFrequency_explosion_000.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserRetro_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/jingles_PIZZA16.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/jingles_NES07.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/powerUp7.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/threeTone2.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/phaserDown3.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/lowDown.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/twoTone1.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/jingles_HIT07.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/jingles_HIT04.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/phaserUp3.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/engineCircular_001.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/laserLarge_002.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/lowFrequency_explosion_001.ogg")
    require(ROOT / "Assets/Resources/Audio/Sfx/jingles_NES03.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/GameOver.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/OutThere.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/MissionPlausible.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/TimeDriving.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/spacelifeNo14.ogg")
    require(ROOT / "Assets/Resources/Audio/Music/SpaceCadet.ogg")
    require(ROOT / "Assets/Resources/Fonts/KenneyFuture.ttf")
    require(ROOT / "Assets/Resources/Fonts/KenneyFutureNarrow.ttf")
    require(ROOT / "Assets/Resources/Fonts/Kenney_Fonts_License.txt")

    version = read(ROOT / "ProjectSettings/ProjectVersion.txt")
    if "6000.6.0f1" not in version:
        err("ProjectVersion.txt should target Unity 6000.6.0f1")

    settings = read(ROOT / "ProjectSettings/ProjectSettings.asset")
    if f"productName: {TITLE}" not in settings:
        err("PlayerSettings productName must be exactly 'Asteroids gone rogue'")
    if "activeInputHandler: 0" not in settings:
        err("activeInputHandler should be 0 (old Input Manager)")

    build = read(ROOT / "ProjectSettings/EditorBuildSettings.asset")
    if "Assets/Scenes/Play.unity" not in build:
        err("EditorBuildSettings must list Assets/Scenes/Play.unity")
    if "3a8c0e1b5d7246f0a2c9d4e6b1f70835" not in build:
        err("EditorBuildSettings scene guid mismatch")

    tags = read(ROOT / "ProjectSettings/TagManager.asset")
    for tag in ("Player", "Enemy", "Asteroid", "Projectile"):
        if f"- {tag}" not in tags:
            err(f"TagManager missing {tag}")

    readme = read(ROOT / "README.md")
    if not readme.startswith(f"# {TITLE}"):
        err("README title must be Asteroids gone rogue")
    if "6000.6.0f1" not in readme:
        err("README must document Unity 6000.6.0f1")

    scripts = list((ROOT / "Assets/Scripts").rglob("*.cs"))
    editor_scripts = list((ROOT / "Assets/Editor").rglob("*.cs"))
    if len(scripts) < 20:
        err(f"expected a full script set, found {len(scripts)}")

    required_types = [
        "class GameSession",
        "class GameManager",
        "class WaveManager",
        "class ShipController",
        "class Asteroid",
        "class EnemySeeker",
        "class HangarShop",
        "class ContentFactory",
        "class ArtImport",
        "class GameBootstrap",
        "class GameUi",
        "class UiTheme",
        "class UiFonts",
        "class HangarPersist",
        "class MedalCatalog",
        "class ArenaLayout",
        "class ArenaHazard",
        "class MonsterPresence",
        "class TelegraphRing",
        "class Loc",
        "class JuiceBurst",
        "class CombatJuice",
        "class DifficultySettings",
        "class GamepadInput",
        "class HangarShipPreview",
        "class ArenaEnv",
        "class AchievementCatalog",
        "class AchievementPersist",
        "class SteamAchievements",
        "class CampaignCap",
        "class HangarPadNav",
        "class StoreCapturePoses",
        "class StoreCaptureDirector",
        "enum GamePhase",
    ]
    blob = "\n".join(read(p) for p in scripts + editor_scripts)
    for token in required_types:
        if token not in blob:
            err(f"missing C# {token}")

    for phase in ("Hangar", "Playing", "WaveClear", "Failed", "CampaignClear"):
        if phase not in blob:
            err(f"state machine missing {phase}")

    bootstrap_guid = guid_for("Assets/Scripts/Content/GameBootstrap.cs")
    meta = ROOT / "Assets/Scripts/Content/GameBootstrap.cs.meta"
    if meta.exists():
        if f"guid: {bootstrap_guid}" not in read(meta):
            err("GameBootstrap.cs.meta guid does not match generator convention")
    else:
        err("GameBootstrap.cs.meta missing")

    scene = read(ROOT / "Assets/Scenes/Play.unity")
    if bootstrap_guid not in scene:
        err("Play.unity does not reference GameBootstrap script guid")
    if "GameSystems" not in scene:
        err("Play.unity missing GameSystems object")
    if "Main Camera" not in scene:
        err("Play.unity missing Main Camera")
    if "EventSystem" not in scene:
        err("Play.unity missing EventSystem")
    if "76c392e42b5098c458856cdf6ecaaaa1" not in scene:
        err("Play.unity EventSystem must reference ugui EventSystem script guid")
    if "4f231c4fb786f3946a6b90b886c48677" not in scene:
        err("Play.unity must persist StandaloneInputModule (ugui package guid) so Hub open has an Input Module")
    if "4f231eb8fc47f54ca11b152d6d181d1e" in scene:
        err("Play.unity still uses the old UnityEngine.UI.dll StandaloneInputModule guid (missing script after Library wipe)")
    if "m_HorizontalAxis: Horizontal" not in scene or "m_VerticalAxis: Vertical" not in scene:
        err("Play.unity StandaloneInputModule must bind Horizontal / Vertical Input Manager axes")
    if "m_SubmitButton: Submit" not in scene or "m_CancelButton: Cancel" not in scene:
        err("Play.unity StandaloneInputModule must bind Submit / Cancel Input Manager buttons")
    if "InputSystemUIInputModule" in scene:
        err("Play.unity must stay on StandaloneInputModule (no Input System UI module)")
    bootstrap_src = read(ROOT / "Assets/Scripts/Content/GameBootstrap.cs")
    if "EnsureEventSystem" not in bootstrap_src:
        err("GameBootstrap must EnsureEventSystem")
    if "GetComponent<StandaloneInputModule>" not in bootstrap_src:
        err("GameBootstrap must repair a scene EventSystem that is missing StandaloneInputModule")
    if "AddComponent<StandaloneInputModule>" not in bootstrap_src:
        err("GameBootstrap must AddComponent StandaloneInputModule")
    if "horizontalAxis" not in bootstrap_src:
        err("GameBootstrap must wire StandaloneInputModule Input Manager axes")
    if "forceModuleActive" in bootstrap_src or "forceModuleActive" in blob:
        err("scripts must not set obsolete StandaloneInputModule.forceModuleActive (CS0619)")
    if "m_ForceModuleActive: 1" in scene:
        err("Play.unity must not force StandaloneInputModule (m_ForceModuleActive: 1)")

    for mat in (
        "Mat_Ship_Hull",
        "Mat_Ship_Accent",
        "Mat_Ship_Glass",
        "Mat_Ship_Glow",
        "Mat_Asteroid",
        "Mat_Enemy",
        "Mat_Arena",
    ):
        require(ROOT / f"Assets/Art/Materials/{mat}.mat")

    factory = read(ROOT / "Assets/Scripts/Content/ContentFactory.cs")
    if "ArtImport.TryInstantiate" not in factory:
        err("ContentFactory should instantiate Import FBX through ArtImport")
    if "Ship_Nose_Upgrade01" not in factory or "Ship_Engine_Upgrade01" not in factory:
        err("ContentFactory should bind hangar upgrade FBX slots")
    if "Ship_Body_Upgrade01" not in factory:
        err("ContentFactory should bind Ship_Body_Upgrade01 for the shop swap")
    if "RosterForWave" not in read(ROOT / "Assets/Scripts/Core/WaveManager.cs"):
        err("WaveManager should expose a Scout/Gunner/Drone ladder")

    art_import = read(ROOT / "Assets/Scripts/Content/ArtImport.cs")
    if "Resources.Load" not in art_import:
        err("ArtImport must Resources.Load Play Mode FBX so Press Play needs no Inspector wiring")
    if "AssetDatabase.LoadAssetAtPath" not in art_import:
        err("ArtImport should also load Assets/Art/Import via AssetDatabase in the Editor")

    play_fbx = (
        "Ship_Nose",
        "Ship_Body",
        "Ship_Body_Upgrade01",
        "Ship_Engine",
        "Ship_Nose_Upgrade01",
        "Ship_Engine_Upgrade01",
        "Enemy_01",
        "Enemy_Scout",
        "Enemy_Gunner",
        "Enemy_Drone",
        "Enemy_Bomber",
        "Enemy_Sniper",
        "Monster_Brute",
        "Monster_Swarm",
        "Arena_Hazard_Spike",
        "Asteroid_Large",
        "Asteroid_Small",
        "Asteroid_VariantB_Large",
        "Asteroid_VariantB_Small",
        "Arena_Blockout",
        "Hangar_Crate",
        "Hangar_Terminal",
        "Hangar_LightPillar",
        "Hangar_Workbench",
        "Hangar_FuelCell",
        "Hangar_ShopKiosk",
        "Hangar_Console",
        "Hangar_PowerBox",
        "Hangar_FireExtinguisher",
        "Hangar_Locker",
        "Hangar_LaunchSign",
        "Ship_Complete",
        "Projectile_Bolt",
        "Projectile_EnemyBolt",
        "Pickup_Score",
        "Pickup_Shield",
    )
    for name in play_fbx:
        path = ROOT / f"Assets/Art/Import/{name}.fbx"
        require(path, "Play Mode mesh")
        if path.exists() and is_lfs_pointer(path):
            err(f"{path.relative_to(ROOT)} looks like an LFS pointer, not an FBX")
        meta = ROOT / f"Assets/Art/Import/{name}.fbx.meta"
        require(meta, "ModelImporter settings")
        if meta.exists():
            text = read(meta)
            if "addColliders: 0" not in text:
                err(f"{name}.fbx.meta should disable generated colliders")
            if "globalScale: 1" not in text:
                err(f"{name}.fbx.meta should import at scale 1")

        resources = ROOT / f"Assets/Resources/Art/Import/{name}.fbx"
        require(resources, "Resources.Load Play Mode mesh")
        if resources.exists() and is_lfs_pointer(resources):
            err(f"{resources.relative_to(ROOT)} looks like an LFS pointer, not an FBX")
        require(ROOT / f"Assets/Resources/Art/Import/{name}.fbx.meta", "Resources ModelImporter")

    for font_name in ("KenneyFuture.ttf", "KenneyFutureNarrow.ttf"):
        font_path = ROOT / "Assets/Resources/Fonts" / font_name
        require(font_path, "bundled HUD font")
        if font_path.exists() and is_lfs_pointer(font_path):
            err(f"{font_path.relative_to(ROOT)} looks like an LFS pointer, not a font")

    resources_import = ROOT / "Assets/Resources/Art/Import"
    for fbx in sorted(resources_import.glob("*.fbx")):
        if is_lfs_pointer(fbx):
            err(f"{fbx.relative_to(ROOT)} is a Git LFS pointer text file, not an FBX binary")

    for prefab in (
        "Ship_Nose",
        "Ship_Body",
        "Ship_Engine",
        "Ship_Nose_Upgrade01",
        "Ship_Engine_Upgrade01",
        "Ship_Complete",
        "Asteroid_Large",
        "Asteroid_Small",
        "Asteroid_VariantB_Large",
        "Asteroid_VariantB_Small",
        "Enemy_01",
        "Arena_Blockout",
        "Hangar_Crate",
        "Hangar_Terminal",
        "Hangar_LightPillar",
    ):
        require(ROOT / f"Assets/Art/Prefabs/{prefab}.prefab")

    # No script should use the new Input System package API.
    if "UnityEngine.InputSystem" in blob:
        err("scripts should stay on the old Input Manager for a clean first open")

    # Unity 6.6 API: Arial builtin is gone; bundled Kenney fonts with LegacyRuntime fallback.
    if "Arial.ttf" in blob or '"Arial"' in blob:
        err("scripts still load builtin/OS Arial; use bundled font or LegacyRuntime.ttf")
    if 'GetBuiltinResource<Font>("LegacyRuntime.ttf")' not in blob and "LegacyBuiltin" not in blob:
        err("scripts should keep LegacyRuntime.ttf as the Unity 6.6 font fallback")
    fonts = read(ROOT / "Assets/Scripts/UI/UiFonts.cs")
    if "Fonts/KenneyFuture" not in fonts or "Fonts/KenneyFutureNarrow" not in fonts:
        err("UiFonts should load bundled Kenney Future / Future Narrow")
    if 'GetBuiltinResource<Font>(LegacyBuiltin)' not in fonts and 'GetBuiltinResource<Font>("LegacyRuntime.ttf")' not in fonts:
        err("UiFonts should fall back to builtin LegacyRuntime.ttf")
    for path in scripts + editor_scripts:
        for hit in shader_conditional_violations(read(path)):
            err(f"{path.relative_to(ROOT)}: {hit} (Unity 6000.6 CS1503)")
        for hit in local_shadow_violations(read(path)):
            err(f"{path.relative_to(ROOT)}: {hit}")

    if "FindObjectOfType" in blob or "FindObjectsOfType" in blob:
        err("scripts still call obsolete FindObjectOfType / FindObjectsOfType")
    if "GetInstanceID" in blob:
        err("scripts still call obsolete GetInstanceID; use GetEntityId")
    if "FindFirstObjectByType" in blob:
        err("scripts still call FindFirstObjectByType; use FindAnyObjectByType")
    for path in scripts:
        src = read(path)
        if "using System;" not in src:
            continue
        for i, line in enumerate(src.splitlines(), 1):
            if "Object.FindAnyObjectByType" in line and "UnityEngine.Object.FindAnyObjectByType" not in line:
                err(
                    f"{path.relative_to(ROOT)}:{i} CS0104: qualify UnityEngine.Object.FindAnyObjectByType"
                )
    if "FindObjectsSortMode" in blob:
        err("scripts still pass FindObjectsSortMode to FindObjectsByType")
    padnav = read(ROOT / "Assets/Scripts/Core/HangarPadNav.cs")
    game_ui = read(ROOT / "Assets/Scripts/UI/GameUi.cs")
    if "ResolveFallback" not in padnav or "StepSelectable" not in padnav or "NavIncludesPrimary" not in padnav:
        err("HangarPadNav must keep Next Wave in the pad order and fall back to it")
    if "NextWaveScreenClear" not in padnav or "LockedShopFallsBackToPrimary" not in padnav:
        err("HangarPadNav should self-check Next Wave clearance and locked-shop fallback")
    if "HangarPadNav.StepSelectable" not in game_ui or "OnHangarEscape" not in game_ui:
        err("GameUi hangar pad must fall back to Next Wave")
    if "OnHangarStart" not in game_ui or "CardIsAction" not in game_ui:
        err("GameUi must keep Start as the launch shortcut and doctrine swap text passive")
    loc_src = read(ROOT / "Assets/Scripts/Core/Loc.cs")
    theme_src = read(ROOT / "Assets/Scripts/UI/UiTheme.cs")
    if "Start launch wave" not in game_ui or "LS move · {0}" not in game_ui:
        err("hangar footer must include Start launch wave on the one-line hint")
    if "Start starta våg" not in loc_src or "LS styr · {0}" not in loc_src:
        err("Swedish hangar footer must include Start starta våg")
    hangar_card = game_ui.split("HangarHintBody")[1].split("FirstWaveCoach")[0]
    if "Abort (Esc)" in hangar_card or "Start = launch wave" not in hangar_card:
        err("first-hangar card must say Start launches the wave, not Abort (Esc)")
    if "B / Esc = focus Next Wave" not in hangar_card:
        err("first-hangar card must say B / Esc focuses Next Wave")
    if "Esc / Start = back to hangar" not in game_ui:
        err("play hint must say Esc / Start = back to hangar")
    if "Esc / Start returns to hangar" not in game_ui:
        err("first-wave coach must say Esc / Start returns to hangar")
    if "HintMin = 18" not in theme_src or "HintSize(int screenWidth)" not in theme_src:
        err("UiTheme.HintMin must be 18 and HintSize(int screenWidth) must exist")
    if "screenWidth <= 1280" not in theme_src or "return HintMin" not in theme_src:
        err("UiTheme.HintSize must return HintMin at width <= 1280")
    if "UiTheme.HintSize(Screen.width)" not in game_ui:
        err("bottom hint and first-hangar card must use UiTheme.HintSize")
    escape_fn = game_ui.split("private void OnHangarEscape()")[1].split("private void")[0]
    if "OnPrimary" in escape_fn:
        err("Esc in the hangar must not launch Next Wave / New Run")
    if "PrimaryRestartsRun" not in read(ROOT / "Assets/Scripts/Core/GameSession.cs"):
        err("primary New Run must be limited to fail and campaign clear")
    if "CardIsAction" not in read(ROOT / "Assets/Scripts/Core/DoctrineRules.cs"):
        err("doctrine 'New Run to swap' must not be an action")

    settings_state = read(ROOT / "Assets/Scripts/Core/SettingsState.cs")
    settings_rows = read(ROOT / "Assets/Scripts/Core/SettingsRows.cs")
    settings_router = read(ROOT / "Assets/Scripts/Core/SettingsInputRouter.cs")
    if "class SettingsState" not in settings_state or "FromInts" not in settings_state:
        err("SettingsState must clamp and round-trip versioned prefs")
    if "ScreenShake = true" not in settings_state or "ConfirmRestartInPlay = true" not in settings_state:
        err("SettingsState defaults must keep shake and in-play restart confirm on")
    if "ConfirmRestartNewRun = false" not in settings_state or "HintMode.HangarFooter" not in settings_state:
        err("SettingsState defaults must keep new-run confirm off and the hangar footer hint")
    if "PadNavSource.Both" not in settings_state or "DefaultHintSizeStep = 1" not in settings_state:
        err("SettingsState must default pad nav to Both and hint size step to 1")
    if "invert" in settings_state.lower():
        err("SettingsState must not add an invert-down option")
    if "class SettingsInputRouter" not in settings_router or "BlocksHangarPad" not in settings_router:
        err("SettingsInputRouter must consume shortcuts while the panel is open")
    if "return flags.Open && !flags.Playing;" not in settings_router:
        err("settings pad lock must cover the open hangar panel only")
    order_block = settings_rows.split("Order =")[1].split(";")[0]
    for row_name in ("Language", "Music", "Sfx", "Mute", "ScreenShake", "Controls", "Close"):
        if row_name not in order_block:
            err(f"settings row list missing {row_name}")
    if "HintMode" in order_block or "PadNav" in order_block:
        err("settings row list must not add hint mode or pad source yet")
    follow = read(ROOT / "Assets/Scripts/Player/FollowCamera.cs")
    if "SettingsState.ScreenShakeEnabled" not in follow or "SettingsState.ShakeAmplitude" not in follow:
        err("FollowCamera.AddShake must read the cached screen-shake setting")
    if "_shake = 0f" not in follow:
        err("FollowCamera must clear residual shake when screen shake is off")
    if "PlayerPrefs" in follow:
        err("FollowCamera must not touch PlayerPrefs on the shake path")
    if "AudioPanel" in game_ui or "BuildAudioControls" in game_ui:
        err("top-bar AudioPanel must be gone; volume lives in the settings panel")
    if "SetMusicVolume" not in game_ui or "SetSfxVolume" not in game_ui or "ToggleMute" not in game_ui:
        err("settings volume rows must call AudioCues SetMusicVolume, SetSfxVolume, and ToggleMute")
    if "MuteSlot" in padnav:
        err("hangar pad must not keep a top-bar mute slot")
    check_loc_key_parity(loc_src)
    if "SettingsSlot" not in padnav or "SettingsGear" not in game_ui:
        err("hangar pad must include the settings gear slot")
    if "JoystickButton6" not in game_ui or "KeyCode.F1" not in game_ui:
        err("hangar settings must toggle from F1 and Select")
    if "FocusHangarSlot(HangarPadNav.SettingsSlot)" not in game_ui:
        err("closing settings must restore the gear slot")
    for settings_key in (
        "ui.settings",
        "ui.settings.language",
        "ui.settings.controls",
        "ui.settings.close",
    ):
        if settings_key not in loc_src or settings_key not in game_ui:
            err(f"missing EN+SV settings key {settings_key}")
    if "Inställningar" not in loc_src or "Stäng" not in loc_src:
        err("Swedish settings copy missing")
    if "SettingsInputRouter.Route" not in game_ui or "OnHangarEscape()" not in game_ui:
        err("GameUi must route settings before hangar Esc")

    if "body.velocity" in blob or "_body.velocity" in blob:
        err("scripts still assign Rigidbody.velocity; use linearVelocity")
    if "body.drag" in blob or "body.angularDrag" in blob:
        err("scripts still use Rigidbody.drag / angularDrag; use linearDamping / angularDamping")

    manifest = read(ROOT / "Packages/manifest.json")
    lock = read(ROOT / "Packages/packages-lock.json")
    if '"com.unity.ugui": "2.0.0"' not in manifest:
        err("Packages/manifest.json should pin com.unity.ugui 2.0.0 for Unity 6")
    if '"com.unity.inputsystem"' in manifest:
        err("do not add the Input System package (keeps first-open clean)")
    if "com.unity.textmeshpro" in manifest or "com.unity.textmeshpro" in lock:
        err("do not add TextMeshPro (bundled Kenney Font + ugui Text stays Hub-safe)")
    for blocked in (
        "com.unity.modules.vr",
        "com.unity.modules.xr",
        "com.unity.modules.cloth",
        "com.unity.modules.terrain",
        "com.unity.modules.vehicles",
        "com.unity.modules.androidjni",
        "com.unity.modules.accessibility",
        "com.unity.modules.umbra",
        "com.unity.modules.unityanalytics",
        "com.unity.modules.tilemap",
        "com.unity.modules.wind",
    ):
        if f'"{blocked}"' in manifest:
            err(f"Packages/manifest.json must not list {blocked} (Hub Continue / unused builtin)")
        if f'"{blocked}"' in lock:
            err(f"Packages/packages-lock.json must not list {blocked}")

    if ERRORS:
        print("Week 1 validation FAILED:")
        for item in ERRORS:
            print(" -", item)
        return 1

    print("Week 1 Unity project structure OK")
    print(f" Title: {TITLE}")
    print(" Unity: 6000.6.0f1")
    print(f" Scripts: {len(scripts)}")
    print(" Scene: Assets/Scenes/Play.unity → GameBootstrap")
    return 0


if __name__ == "__main__":
    sys.exit(main())
