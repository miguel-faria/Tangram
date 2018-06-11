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
		//private GameObject _last_object_touched = null;
        private GameObject _last_object_collided = null;
		private Vector2 _final_movement_pos;
        private Vector3 _new_piece_position;
        private AudioSource _sound_source;
		
        //public Transform _edge_particles;
        public AudioClip _placed_audio;

		// Use this for initialization
		void Start () {

            _sound_source = GetComponent<AudioSource> ( );

		}
		
		// Update is called once per frame
		void Update () {
			if (_unlocked) {
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

			} else if(_position_changed) {

                //transform.position = Vector3.MoveTowards(transform.position, _new_piece_position, 0.05f);
                transform.position = Vector3.MoveTowards(transform.position, _new_piece_position, 5*Time.deltaTime);
                if (Vector3.Distance(transform.position, _new_piece_position) < 0.05f) {
                    Transform piece_solution = GameObject.Find("Solution").transform.Find("socket_" + gameObject.name);
                    transform.position = _new_piece_position;
                    transform.rotation = piece_solution.rotation;
                    piece_status = (int)Piece_States.LOCKED;
                    piece_solution.GetComponent<PolygonCollider2D>().enabled = false;
                    GetComponent<PolygonCollider2D>().enabled = false;
                    GetComponent<Renderer>().sortingOrder = -1;
                    //Instantiate(_edge_particles, other.gameObject.transform.position, _edge_particles.rotation);
                    _sound_source.PlayOneShot(_placed_audio, 1.0f);
                    PuzzleManager.Instance.piece_placed(gameObject.name);
                    _position_changed = false;
                }
            }

		}

		void OnTriggerStay2D(Collider2D other){
            
            _collided = true;
            //Debug.Log("HEY!!!");
            //Debug.Log(other.gameObject.name);

            if (piece_status == (int)Piece_States.JUST_MOVED){

			    if (other.gameObject.name == "socket_" + gameObject.name) {

				    transform.position = other.gameObject.transform.position;
                    transform.rotation = other.gameObject.transform.rotation;
                    piece_status = (int)Piece_States.LOCKED;
				    other.GetComponent<PolygonCollider2D> ().enabled = false;
				    GetComponent<PolygonCollider2D> ().enabled = false;
                    GetComponent<Renderer>().sortingOrder = -1;
                    //Instantiate(_edge_particles, other.gameObject.transform.position, _edge_particles.rotation);
                    _sound_source.PlayOneShot (_placed_audio, 1.0f);
	                PuzzleManager.Instance.piece_placed(gameObject.name);
				    //PuzzleManager.Instance.reset_piece_timer ();
                    //GameManager.Instance.get_logger ( ).write_log_line ("Correctly placed piece: " + gameObject.name);

                } else if(other.gameObject.name.Contains("socket")) {
                    
                    piece_status = (int) Piece_States.IDLE;

                    if (other.gameObject != _last_object_collided) {
                        //PuzzleManager.Instance.new_error ( );
                        _last_object_collided = other.gameObject;
                        _just_errored = true;
                        //Debug.Log ("Kid dropped piece in wrong place! Number of Wrong Times: " + PuzzleManager.Instance.get_n_errors());
                        //GameManager.Instance.get_logger().write_log_line("Piece " + gameObject.name + " dropped in wrong place! Number of Wrong Tries: " + PuzzleManager.Instance.get_n_errors());
                    } else {
                        if (!_just_errored) {
                            //PuzzleManager.Instance.new_error ( );
                            _just_errored = true;
                            //Debug.Log ("Kid dropped piece in wrong place! Number of Wrong Times: " + PuzzleManager.Instance.get_n_errors ( ));
                            //GameManager.Instance.get_logger ( ).write_log_line ("Piece " + gameObject.name + " dropped in wrong place! Number of Wrong Tries: " + PuzzleManager.Instance.get_n_errors ( ));
                        } else {
                            _just_errored = false;
                        }

                    }

                }

            }

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
					GetComponent<Renderer> ().sortingOrder = 10;
					//PuzzleManager.Instance.control_piece_timer (false);
					_piece_cliked = true;
                    //GameManager.Instance.get_logger ( ).write_log_line ("Picked piece: " + gameObject.name);
				}
			}

		}

		void OnMouseUp(){
			if (_unlocked){
		        if ((piece_status != (int)Piece_States.LOCKED) && Input.GetMouseButtonUp (0)) {
					piece_status = (int)Piece_States.JUST_MOVED;
					GetComponent<Renderer> ().sortingOrder = 0;
					//PuzzleManager.Instance.control_piece_timer (true);
					_piece_cliked = false;
                    _just_moved = true;
                    //GameManager.Instance.get_logger ( ).write_log_line ("Dropped piece: " + gameObject.name);
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

        public void piece_position(Vector3 new_position) {
            _new_piece_position = new_position;
        }
			
	}
}
