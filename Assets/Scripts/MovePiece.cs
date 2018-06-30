using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangram{

	public class MovePiece : MonoBehaviour {
		
		private int piece_status = (int)Piece_States.IDLE;
		private bool _unlocked = true;
		private bool _piece_cliked = false;
        private bool _just_errored = false;
        private bool _just_moved = false;
        private bool _collided = false;
        private bool _position_changed = false;
        private bool _rotation_changed = false;
		//private GameObject _last_object_touched = null;
        private GameObject _last_object_collided = null;
		private Vector2 _final_movement_pos;
        private Vector3 _new_piece_position;
        private Quaternion _new_piece_rotation;
        private AudioSource _sound_source;
        private RotatePiece _rotate_piece_ref;
		
        //public Transform _edge_particles;
        public AudioClip _placed_audio;

		// Use this for initialization
		void Start () {

            _sound_source = GetComponent<AudioSource> ( );
            _rotate_piece_ref = gameObject.GetComponentInChildren<RotatePiece>();

		}
		
		// Update is called once per frame
		void Update () {
			if (_unlocked) {
                if (GameManager.Instance.play_with_rotation() && _rotate_piece_ref.need_rotate()) {

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, _new_piece_rotation, 50 * Time.deltaTime);
                    if (transform.rotation == _new_piece_rotation)
                        _rotate_piece_ref.set_rotate(false);

                } else { 
				    /*if(Input.touchCount > 0) {

					    Touch touch = Input.GetTouch (0);

					    switch(touch.phase){

					    case TouchPhase.Began:

						    RaycastHit2D hit = Physics2D.Raycast (Camera.main.ScreenToWorldPoint (Input.mousePosition), Vector2.zero);

						    if (hit){
							    _last_object_touched = hit.collider.gameObject;
							    if((_last_object_touched == gameObject) && (piece_status == (int)Piece_States.IDLE) && (piece_status != (int)Piece_States.LOCKED)){
								    piece_status = (int)Piece_States.PICKED;
								    _final_movement_pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
								    GetComponent<Renderer> ().sortingOrder = 10;
								    PuzzleManager.Instance.control_piece_timer (false);
							    }
						    }

						    break;

					    case TouchPhase.Moved:

						    if(_last_object_touched != null && gameObject == _last_object_touched && (piece_status != (int)Piece_States.LOCKED)){
							    _final_movement_pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
						    }

						    break;


					    case TouchPhase.Ended:

						    if(_last_object_touched != null && gameObject == _last_object_touched && (piece_status != (int)Piece_States.LOCKED)){
							    _final_movement_pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
							    piece_status = (int)Piece_States.IDLE;
							    GetComponent<Renderer> ().sortingOrder = 0;
							    PuzzleManager.Instance.control_piece_timer (true);
						    }

						    break;
					    }

				    }else {*/
				    if (_piece_cliked)
					    _final_movement_pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
				    //}

				    if (piece_status == (int)Piece_States.PICKED) {
					    Vector2 obj_pos = Camera.main.ScreenToWorldPoint (_final_movement_pos);
					    transform.position = obj_pos;
				    }

                    if(_just_moved && !_collided){
                        _just_errored = false;
                        _just_moved = false;
                        _collided = false;
                    }
                }

            } else if(_position_changed) {

                transform.position = Vector3.MoveTowards(transform.position, _new_piece_position, 5*Time.deltaTime);
                if (Vector3.Distance(transform.position, _new_piece_position) < 0.05f) {
                    Transform piece_solution = GameObject.Find("Solution").transform.Find("socket_" + gameObject.name);
                    put_piece_place(piece_solution, _new_piece_position);
                }
                _position_changed = false;

            } else if (_rotation_changed) {

                transform.rotation = Quaternion.RotateTowards(transform.rotation, _new_piece_rotation, 25 * Time.deltaTime);
                transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                if(transform.rotation == _new_piece_rotation) {
                    transform.rotation = _new_piece_rotation;
                }
                _rotation_changed = false;

            }

		}

		void OnTriggerStay2D(Collider2D other){
            
            _collided = true;

            if (piece_status == (int)Piece_States.JUST_MOVED){
                
			    if (other.gameObject.name == "socket_" + gameObject.name) {

                    put_piece_place(other.gameObject.transform, other.gameObject.transform.position);

                } else if(other.gameObject.name.Contains("socket")) {

                    if (GameManager.Instance.get_difficulty() == (int)Difficulty_Levels.HARD && possible_location(other.gameObject)) {

                        Dictionary<string, GameObject> solution = PuzzleManager.Instance.get_solution_pieces();
                        List<string> current_location = new List<string> (other.gameObject.name.Split('_'));
                        string location_name = "";
                        foreach (string name_part in current_location) {
                            if (!name_part.Contains("socket")) { 
                                location_name += name_part;
                                if (name_part != current_location[current_location.Count -1])
                                    location_name += "_";
                            }
                        }
                        GameObject place_object_location = solution[location_name];
                        GameObject picked_object_location = solution[gameObject.name];
                        solution.Remove(location_name);
                        solution.Remove(gameObject.name);
                        solution.Add(location_name, picked_object_location);
                        solution.Add(gameObject.name, place_object_location);
                        PuzzleManager.Instance.set_solution_pieces(solution);
                        put_piece_place(other.gameObject.transform, other.gameObject.transform.position);

                    } else {

                        piece_status = (int) Piece_States.IDLE;

                        if (other.gameObject != _last_object_collided) {
                            _last_object_collided = other.gameObject;
                            _just_errored = true;
                        } else {
                            if (!_just_errored) {
                                _just_errored = true;
                            } else {
                                _just_errored = false;
                            }

                        }
                    }

                }

            }

		}

        private bool possible_location(GameObject location) {

            List<string> collided_object_name = new List<string> (location.name.Split('_'));
            string[] object_name = gameObject.name.Split('_');

            if (gameObject.transform.rotation == location.transform.rotation && collided_object_name.IndexOf(object_name[0]) > -1 && collided_object_name.IndexOf(object_name[1]) > -1)
                return true;
            else
                return false;

        }

        private void put_piece_place(Transform solution_location, Vector3 final_pos) {

            transform.position = final_pos;
            transform.rotation = solution_location.rotation;
            piece_status = (int)Piece_States.LOCKED;
            solution_location.GetComponent<PolygonCollider2D>().enabled = false;
            GetComponent<PolygonCollider2D>().enabled = false;
            GetComponent<Renderer>().sortingOrder = -1;
            transform.GetChild(0).gameObject.SetActive(false);
            //Instantiate(_edge_particles, other.gameObject.transform.position, _edge_particles.rotation);
            _sound_source.PlayOneShot(_placed_audio, 1.0f);
            PuzzleManager.Instance.piece_placed(gameObject.name);

        }

        void OnCollisionEnter (Collision collision) {
            _collided = true;
        }

        void OnCollisionExit (Collision collision) {
            _collided = false;
        }

		void OnMouseDown(){
	        if(_unlocked){
				if ((piece_status != (int)Piece_States.LOCKED) && Input.GetMouseButtonDown (0)){
                    piece_status = (int)Piece_States.PICKED;
					GetComponent<Renderer> ().sortingOrder = 20;
					_piece_cliked = true;
				}
			}

		}

		void OnMouseUp(){
			if (_unlocked){
		        if ((piece_status != (int)Piece_States.LOCKED) && Input.GetMouseButtonUp (0)) {
					piece_status = (int)Piece_States.JUST_MOVED;
					GetComponent<Renderer> ().sortingOrder = 1;
					_piece_cliked = false;
                    _just_moved = true;
				}
			}
		}

		public void lock_piece(){
			_unlocked = false;
		}

		public void unlock_piece(){
			_unlocked = true;
		}

        public void set_position_change(bool change) {
            _position_changed = change;
        }

        public void set_rotation_change(bool change) {
            _rotation_changed = change;
        }

        public void piece_position(Vector3 new_position) {
            _new_piece_position = new_position;
        }
			
        public void rotation_objective(Quaternion new_rotation) {
            _new_piece_rotation = new_rotation;
        }

	}
}
