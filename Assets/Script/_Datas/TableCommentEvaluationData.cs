using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TableCommentEvaluationData
{

    public int ID;
    public string Message;
    public int StarValue;
    public TableCommentEvaluationData(int _iD, string _message, int _starValue)
    {
        this.ID = _iD;
        this.Message = _message;
        this.StarValue = _starValue;        
    }

    public TableCommentEvaluationData(TableCommentEvaluationData _evaluation)
    {
        this.ID = _evaluation.ID;
        this.Message = _evaluation.Message;
        this.StarValue = _evaluation.StarValue;        
    }
}
    
