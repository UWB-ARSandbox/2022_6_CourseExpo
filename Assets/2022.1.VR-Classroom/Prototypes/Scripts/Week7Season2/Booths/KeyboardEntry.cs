using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardEntry : MonoBehaviour
{
    InputField txtField;
    public Button A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z;
    public Button num1, num2, num3, num4, num5, num6, num7, num8, num9, num0;
    public Button add, sub, mult, div, dec, equals;
    public Button btnBackspace;
    public Button btnSpacebar;

    public CollaborativeManager _InputInterceptor;

    // Start is called before the first frame update
    void Start()
    {
        _InputInterceptor = gameObject.transform.parent.transform.parent.transform.parent.transform.parent.GetComponent<CollaborativeManager>();
        txtField = GetComponentInChildren<InputField>();
        A.onClick.AddListener(() => AddChar("A"));
        B.onClick.AddListener(() => AddChar("B"));
        C.onClick.AddListener(() => AddChar("C"));
        D.onClick.AddListener(() => AddChar("D"));
        E.onClick.AddListener(() => AddChar("E"));
        F.onClick.AddListener(() => AddChar("F"));
        G.onClick.AddListener(() => AddChar("G"));
        H.onClick.AddListener(() => AddChar("H"));
        I.onClick.AddListener(() => AddChar("I"));
        J.onClick.AddListener(() => AddChar("J"));
        K.onClick.AddListener(() => AddChar("K"));
        L.onClick.AddListener(() => AddChar("L"));
        M.onClick.AddListener(() => AddChar("M"));
        N.onClick.AddListener(() => AddChar("N"));
        O.onClick.AddListener(() => AddChar("O"));
        P.onClick.AddListener(() => AddChar("P"));
        Q.onClick.AddListener(() => AddChar("Q"));
        R.onClick.AddListener(() => AddChar("R"));
        S.onClick.AddListener(() => AddChar("S"));
        T.onClick.AddListener(() => AddChar("T"));
        U.onClick.AddListener(() => AddChar("U"));
        V.onClick.AddListener(() => AddChar("V"));
        W.onClick.AddListener(() => AddChar("W"));
        X.onClick.AddListener(() => AddChar("X"));
        Y.onClick.AddListener(() => AddChar("Y"));
        Z.onClick.AddListener(() => AddChar("Z"));
        num1.onClick.AddListener(() => AddChar("1"));
        num2.onClick.AddListener(() => AddChar("2"));
        num3.onClick.AddListener(() => AddChar("3"));
        num4.onClick.AddListener(() => AddChar("4"));
        num5.onClick.AddListener(() => AddChar("5"));
        num6.onClick.AddListener(() => AddChar("6"));
        num7.onClick.AddListener(() => AddChar("7"));
        num8.onClick.AddListener(() => AddChar("8"));
        num9.onClick.AddListener(() => AddChar("9"));
        num0.onClick.AddListener(() => AddChar("0"));
        add.onClick.AddListener(() => AddChar("+"));
        sub.onClick.AddListener(() => AddChar("-"));
        mult.onClick.AddListener(() => AddChar("*"));
        div.onClick.AddListener(() => AddChar("/"));
        dec.onClick.AddListener(() => AddChar("."));
        equals.onClick.AddListener(() => AddChar("="));
        btnSpacebar.onClick.AddListener(() => AddChar(" "));
        btnBackspace.onClick.AddListener(Backspace);
        if(_InputInterceptor != null){
            _InputInterceptor.txtField = txtField;
        }
    }

    public void AddChar(string character) {
        // if(_InputInterceptor != null){
        //     ExternalUpdate(character);
        // }
        // else
            txtField.text += character;
    }
    public void ExternalUpdate(string character){
        _InputInterceptor.SendTextUpdates(character);
    }

    public void Backspace() {        
        // if(_InputInterceptor != null){
        //     _InputInterceptor.SendBackSpace();
        // }
        // else
            txtField.text = txtField.text.Remove(txtField.text.Length - 1);
    }
}
