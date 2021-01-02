using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            resetScene();
        }
    }

    public void resetScene()
    {
        SceneManager.LoadScene("newmapscene");
    }
    public void tuckButton(bool tuck)
    {
        PlayerController.instance.tuckButton = tuck;
    }
    public void archButton(bool arch)
    {
        PlayerController.instance.archButton = arch;
    }
    public void letGoButton(bool letGo)
    {
        PlayerController.instance.letGoButton = letGo;
    }
}