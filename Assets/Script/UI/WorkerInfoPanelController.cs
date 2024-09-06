using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class WorkerInfoPanelController : MonoBehaviour
{
    [SerializeField] Text txtLevel;
    [SerializeField] Text txtName;
    [SerializeField] Text txtJob;
    [SerializeField] Text txtAge;
    [SerializeField] Text txtHeight;
    [SerializeField] Text txtSalary;
    [SerializeField] Text txtRoomCode;

    [SerializeField] Image imgExpFilleropenStartsContent;

    [SerializeField] Transform openStarsContent;
    [SerializeField] Transform closeStarsContent;

    [SerializeField] Button btnDismissWorker;
    [SerializeField] Button btnExit;

    private WorkerBehaviour currentWorker;

    private void Start()
    {
        btnDismissWorker.onClick.AddListener(DissmisWorker);
        btnExit.onClick.AddListener(ExitPanel);
    }
    public void SetWorker(WorkerBehaviour _worker)
    {
        currentWorker = _worker;
        Worker w = currentWorker.MyScript;
        txtName.text = w.Name;
        txtJob.text = WorkerManager.instance.GetWorkerTypeFormatToString(w.WorkerType);
        txtAge.text = w.Age.ToString();
        txtHeight.text = w.Height.ToString();
        RoomCell roomCell = RoomManager.instance.GetRoomCellWithID(w.IWorkRoomsIDs[0]);
        txtRoomCode.text = roomCell.CellLetter.ToString() + roomCell.CellNumber.ToString();
    }
    void SetUIS()
    {
        if (currentWorker != null)
        {
            if (!currentWorker.forWorkerInfoPanelCam.gameObject.activeSelf)
                currentWorker.forWorkerInfoPanelCam.gameObject.SetActive(true);

            Worker w = currentWorker.MyScript;
            txtLevel.text = w.Level.ToString();
            
            txtSalary.text = (currentWorker.MyDatas.baseSalary * ((currentWorker.StarRank + 1) / 0.5f) * (currentWorker.NotPaidCounter > 0 ? currentWorker.NotPaidCounter : 1)).ToString();

            float requiredExp = WorkerManager.instance.GetRequiredNextLevelExp(w.Level);
            if (requiredExp > 0)
            {
                imgExpFilleropenStartsContent.fillAmount = Mathf.Clamp(w.Exp / requiredExp, 0, 1);
            }
            else
            {
                imgExpFilleropenStartsContent.fillAmount = 0;
            }

            int starRank = currentWorker.StarRank;
            CloseAllStars();
            for (int i = 0; i < starRank; i++)
            {
                if(!openStarsContent.GetChild(i).gameObject.activeSelf)
                openStarsContent.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
    void CloseAllStars()
    {
        int length = openStarsContent.childCount;
        for (int i = 0; i < length; i++)
        {
            openStarsContent.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void ExitPanel()
    {
        gameObject.SetActive(false);
    }
    public void DissmisWorker()
    {
        WorkerManager.instance.TransferCurrentWorkerToInventory(currentWorker.MyScript.ID);
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(SetUIS), 0, 0.1f);
    }

    private void OnDisable()
    {
        currentWorker.forWorkerInfoPanelCam.gameObject.SetActive(false);
        currentWorker = null;
    }
}
