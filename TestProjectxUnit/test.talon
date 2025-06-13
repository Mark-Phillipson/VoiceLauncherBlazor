app.name: Company of Heroes 2
-
settings():
    user.mode_indicator_show = 0
    # Choose how pop click should work in 'control mouse' mode
    # 0 = off
    # 1 = on with eyetracker but not zoom mouse mode
    # 2 = on but not with zoom mouse mode
    user.mouse_enable_pop_click = 0
    key_hold = 32
    # Zoom Mouse Options     
    tracking.zoom_live = true
    tracking.zoom_height = 300
    tracking.zoom_width = 300
    tracking.zoom_scale = 4
    

# This is a list of the commands that are specific to the Company of Heroes 2 game
game <user.arrow_key>:
    key(arrow_key)
    repeat(4)
fly <user.arrow_key>:
    key(arrow_key)
    repeat(7)
    sleep(50ms)
    key(arrow_key)
    repeat(7)
    sleep(50ms)
# Basic commands
move forward:
    key(up)
move back:
    key(down)
move left:
    key(left)
move right:
    key(right)
attack [move]:
    key(a)
    mouse_click(0)
stop:
    key(s)
retreat:
    key(t)
reinforce:
    key(r)
reverse drive:
    key(u)
primary build:
    key(v)
secondary build:
    key(b)
headquarters:
    key(f1)
infantry:
    key(f2)
support:
    key(f3)
light [vehicles]:
    key(f4)
heavy [vehicles]:
    key(f5)
[set] rally point:
    key(ctrl-a)
    sleep(30ms)
    mouse_click(0)
[toggle] map:
    key(keypad_0)
go there:
    mouse_click(0)
    key(keypad_0)
[next] idle vehicle:
    key(alt-/)
[next] idle [soldier]:
    key(alt-.)
idle: key(alt-.)
[all] vehicles:
    key(ctrl-/)
queue: 
    key(shift:down)
    sleep(30ms)
    mouse_click(1)
    sleep(30ms)
    key(shift:up)
next:
    key(shift:down)
    sleep(30ms)
    mouse_click(0)
    sleep(30ms)
    key(shift:up)
explode:
    key(ctrl-/)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
hold shift:
    key(shift:down)
release shift:
    key(shift:up)    
[all] soldiers:
    key(ctrl-.)
[all] invade:
    key(ctrl-.)   
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
reset camera:
    key(backspace)
[game] pause:
    key(pause)
game stop:
    key(pause)
pause game:
    key(pause)
[game] menu:
    key(f10)
take [all]:
    mouse_move(60, 40)
    sleep(30ms)
    user.mouse_drag(0)
    sleep(30ms)
    mouse_move(1670, 250)
    sleep(30ms)
    mouse_move(1670, 979)
    sleep(30ms)
    mouse_click(0)
#save game: key(f5)
unit one: key(1)
unit two: key(2)
unit three: key(3)
unit four: key(4)
unit five: key(5)
unit six: key(6)
unit seven: key(7)
unit eight: key(8)
unit nine: key(9)
unit zero: key(0)
centre:
    mouse_move(800, 500)
go home: key(home)
[unit] one view:
    key(1)
    key(1)
[unit] two view:
    key(2)
    key(2)
[unit] three view:
    key(3)
    key(3)
[unit] four view:
    key(4)
    key(4)
[unit] five view:
    key(5)
    key(5)
[unit] six view:
    key(6)
    key(6)
[unit] seven view:
    key(7)
    key(7)
[unit] eight view:
    key(8)
    key(8)
[unit] nine view:
    key(9)
    key(9)
[unit] zero view:
    key(0)
    key(0)
slot first:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(522, 862)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
slot second:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(602, 853)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
slot third:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(680, 852)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
slot fourth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(766, 856)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
slot fifth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(852, 859)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
zoom out: user.mouse_scroll_down()
zoom: user.mouse_scroll_up()
touch first: 
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1890, 115)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
touch second:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1830, 120)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
touch third:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1780, 120)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
touch fourth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1720, 120)
    sleep(30ms)
    mouse_click(0)  
    mouse_move(position_x , position_y )
touch fifth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1660, 120)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
touch sixth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1610, 120)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
touch seventh:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1550, 120)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
touch eighth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1500, 120)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
touch ninth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1440, 120)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
get stuff:
    mouse_move(380, 855)
    sleep(30ms)
    mouse_click(0)
vehicle first: 
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1885, 225)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
vehicle second:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1835, 230)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
vehicle third:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1785, 230)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
vehicle fourth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1720, 230)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
vehicle fifth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1795, 230)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
vehicle sixth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1745, 230)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
vehicle seventh:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1695, 230)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
emplacement first:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1885, 345)
    sleep(30ms)
    mouse_click(0)    
    mouse_move(position_x , position_y )
emplacement second:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1835, 350)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
emplacement third:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1780, 340)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
emplacement fourth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1720, 340)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
emplacement fifth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1790, 340)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
emplacement sixth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1740, 340)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
emplacement seventh:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1690, 340)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
emplacement eighth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1640, 340)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
emplacement ninth:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1590, 340)
    sleep(30ms)
    mouse_click(0)
    mouse_move(position_x , position_y )
group one:
    key(shift-1)
    key(ctrl-1)    
group two:
    key(shift-2)
    key(ctrl-2)
group three:
    key(shift-3)
    key(ctrl-3)
group four:
    key(shift-4)
    key(ctrl-4)
group five:
    key(shift-5)
    key(ctrl-5)
group six:
    key(shift-6)
    key(ctrl-6)
group seven:
    key(shift-7)
    key(ctrl-7)
group eight:
    key(shift-8)
    key(ctrl-8)
group nine:
    key(shift-9)
    key(ctrl-9) 
group zero:
    key(shift-0)
    key(ctrl-0)
kill tank:
    key(7)    
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
reverse:
    key(u)
    sleep(30ms)
    mouse_click(0)
antitank:
    key(7)
[get] gammon [bomb]:
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(1790, 1025)
    sleep(30ms)
    mouse_click(0)
    sleep(30ms)
    mouse_move(position_x , position_y )
one attack:
    key(1)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
two attack:
    key(2)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
three attack:
    key(3)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
four attack:
    key(4)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
five attack:
    key(5)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
six attack:
    key(6)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
seven attack:
    key(7)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
eight attack:
    key(8)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
nine attack:
    key(9)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
zero attack:
    key(0)
    sleep(30ms)
    key(a)
    sleep(30ms)
    mouse_click(0)
rotate small:
    key(alt:down)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    adjustment = 100
    sleep(300ms)
    mouse_move(position_x - adjustment , position_y)
    sleep(300ms)
    key(alt:up)
rotate right:
    key(alt:down)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    adjustment = 8000
    sleep(300ms)
    mouse_move(position_x - adjustment , position_y)
    sleep(300ms)
    key(alt:up)
rotate left:
    key(alt:down)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    adjustment = 8000
    sleep(300ms)
    mouse_move(position_x + adjustment , position_y)
    sleep(300ms)
    key(alt:up)
six o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x, position_y + 300)
    sleep(900ms)
    user.mouse_drag_end()
twelve o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x, position_y - 300)
    sleep(700ms)
    user.mouse_drag_end()
nine o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x - 300, position_y)
    sleep(700ms)
    user.mouse_drag_end()
three o'clock:
    sleep(100ms)
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x + 300, position_y)
    sleep(700ms)
    user.mouse_drag_end()
one o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x + 66, position_y - 132)
    sleep(700ms)
    user.mouse_drag_end()
two o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x + 132, position_y - 66)
    sleep(700ms)
    user.mouse_drag_end()
four o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x + 132, position_y + 66)
    sleep(700ms)
    user.mouse_drag_end()
five o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x + 132, position_y + 66)
    sleep(700ms)
    user.mouse_drag_end()
seven o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x - 66, position_y + 132)
    sleep(700ms)
    user.mouse_drag_end()
eight o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x - 132, position_y + 66)
    sleep(700ms)
    user.mouse_drag_end()
ten o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x - 132, position_y - 66)
    sleep(700ms)
    user.mouse_drag_end()
eleven o'clock:
    user.mouse_drag(1)
    sleep(100ms)
    position_x = user.query_mouse_position_x()
    position_y = user.query_mouse_position_y()
    mouse_move(position_x - 66, position_y - 132)
    sleep(700ms)
    user.mouse_drag_end()
one back:
    key(1)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
two back:
    key(2)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
three back:
    key(3)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
four back:
    key(4)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
five back:
    key(5)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
six back:
    key(6)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
seven back:
    key(7)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
eight back:
    key(8)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
nine back:
    key(9)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)
zero back:
    key(0)
    sleep(30ms)
    key(u)
    sleep(30ms)
    mouse_click(0)