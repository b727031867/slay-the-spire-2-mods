extends AnimationPlayer
func _ready():
    animation_finished.connect(_return_to_idle)
func _return_to_idle(animation_name: StringName):
    if animation_name != &"Dead":
        play(&"Idle")
