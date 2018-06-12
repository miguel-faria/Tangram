using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tangram { 

    public class RotatePiece : MonoBehaviour {

        private bool _need_rotate = false;
        private MovePiece _move_piece_ref;

	    // Use this for initialization
	    void Start () {
            _move_piece_ref = gameObject.GetComponentInParent<MovePiece>();
	    }
	
	    // Update is called once per frame
	    void Update () {
		
	    }

        void OnMouseDown() {
            if (Input.GetMouseButtonDown(0)) { 
                _need_rotate = true;
                _move_piece_ref.rotation_objective(gameObject.transform.parent.rotation * Quaternion.Euler(0, 0, -45));
            }
        }

        public void set_rotate(bool rotate) {
            _need_rotate = rotate;
        }

        public bool need_rotate() {
            return _need_rotate;
        }
    }
}