extends AnimationPlayer
func _ready():
    animation_started.connect(_start_pose)
    animation_finished.connect(_return_to_idle)
func _start_pose(animation_name: StringName):
    get_node("../Visuals").call("PlayAction", str(animation_name))
func _return_to_idle(animation_name: StringName):
    if animation_name != &"Dead":
        play(&"Idle")
