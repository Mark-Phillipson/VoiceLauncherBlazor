from talon import Module

mod = Module()


@mod.capture(rule="{user.letter} | {user.symbol_key}")
def cursorless_take_target_key(m) -> str:
    return str(m)
