extends Resource

@export var auto_skip_time : float = 4.0
@export_multiline var lines : PackedStringArray

var is_finished = false

var _current_line = 0
var _auto_skip_countdown = -1.0


func start():
	is_finished = false
	_current_line = -1
	next_line()

func process(delta):
	_auto_skip_countdown -= delta
	if _auto_skip_countdown <= 0:
		next_line()

func next_line():
	_current_line += 1

	if _current_line >= lines.size():
		stop()
		return

	_auto_skip_countdown = auto_skip_time
	Resources.Subtitles.Show(lines[_current_line])

func stop():
	Resources.Subtitles.Stop()
	is_finished = true
