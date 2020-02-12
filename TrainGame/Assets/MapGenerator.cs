using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int lines = 3;
    [SerializeField] private int length = 10;

    private int [,]map;

    private bool[,] canBeChanged;
    private bool[,] reversed;

    private MeshRenderer renderer;
    private List<Transform> tracks;
    private Ray ray;

    [SerializeField] private Transform initialTrack;
    [SerializeField] private float distance;
    [SerializeField] private float scaler;
    // Start is called before the first frame update
    void Start()
    {
        initialTrack.localScale = new Vector3(scaler, scaler, scaler);
        renderer = initialTrack.GetComponent<MeshRenderer>();
        tracks = new List<Transform>();
        if(lines < 1)
        lines = 1;
        if(length < 1)
        length = 1;
        map = new int[lines, length];
        canBeChanged = new bool[lines, length];
        reversed = new bool[lines, length];
        GenerateMap();

    }

    private void GenerateMap()
    {
        for (int i = 0; i < lines; i++)
        {
            BuildTrack(i);
        }
    }

    private void SetColor(MeshRenderer[] renderers, float alpha)
    {
        foreach(MeshRenderer renderer in renderers)
        {
            ToFadeMode(renderer.material);
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
        }
    }

    private void ChechTrack(Transform track, int x, int y)
    {
        
        MeshRenderer[] straight = track.Find("Straight").GetComponentsInChildren<MeshRenderer>();
        MeshRenderer[] up = track.Find("Up").GetComponentsInChildren<MeshRenderer>();
        MeshRenderer[] down = track.Find("Down").GetComponentsInChildren<MeshRenderer>();

        //Debug.Log(canBeChanged[y, x]);

        if(canBeChanged[y,x])

        switch (map[y, x])
        {
            case 1: track.Find("Ground").GetComponentInChildren<MeshRenderer>().material.color = Color.red; SetColor(up, 1f);   SetColor(down, 0.1f); SetColor(straight, 0.1f); break;
            case 2: track.Find("Ground").GetComponentInChildren<MeshRenderer>().material.color = Color.yellow; SetColor(up, 0.1f); SetColor(down, 0.1f); SetColor(straight, 1f);   break;
            case 3: track.Find("Ground").GetComponentInChildren<MeshRenderer>().material.color = Color.green; SetColor(up, 0.1f); SetColor(down, 1f);   SetColor(straight, 0.1f); break;
            default: SetColor(up, 0); SetColor(down, 0); SetColor(straight, 0); break;
        }

        else
        {
            track.Find("Ground").GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
            SetColor(up, 0f);
            SetColor(down, 0f);
            SetColor(straight, 1f);
        }
    }

    private void BuildTrack(int line)
    {
        for(int i = 0; i < length; i++)
        {
            Transform track = Instantiate(initialTrack, new Vector3(line * distance, 0, i * distance), Quaternion.identity);
            tracks.Add(track);
            

            if (line > 0 && map[line - 1, i] == 3 && tracks[(line * length + i) - length].eulerAngles.y != 180)
            {
                canBeChanged[line, i] = reversed[line, i] = true;
                map[line, i] = 3;
                tracks[line * length + i].Rotate(0, 180, 0);
                ChechTrack(tracks[(line * length + i)], i, line);
            }

            else if (Random.Range(1, 100) > 75 && line > 0 && map[line - 1, i] != 3 && map[line - 1, i] != 1)
            {
                canBeChanged[line, i] = canBeChanged[line - 1, i] = true;
                map[line, i] = map[line - 1, i] = 1;
                tracks[(line * length + i) - length].Rotate(0, 180, 0);
                ChechTrack(tracks[(line * length + i) - length], i, line - 1);
                ChechTrack(track, i, line);
            }

            else if(Random.Range(1,100) > 75 && line < lines - 1)
            {
                if (line == 0)
                {
                    canBeChanged[line, i] = reversed[line, i] = true;
                    map[line, i] = 3;
                    ChechTrack(track, i, line);
                }

                else
                {
                    if(map[line - 1, i] == 2)
                    {
                        canBeChanged[line, i] = reversed[line, i] = true;
                        map[line, i] = 3;
                        ChechTrack(track, i, line);
                    }

                    else
                    {
                        map[line, i] = 2;
                        ChechTrack(track, i, line);
                    }
                }
            }

            else
            {
                map[line, i] = 2;
                ChechTrack(track, i, line);
            }

        }
    }

    private void ClearMap()
    {
        for(int i = tracks.Count - 1; i >= 0; i--)
        {
            Destroy(tracks[i].gameObject);
            tracks.Remove(tracks[i]);
        }

        for(int i = 0; i < lines; i++)
        {
            for(int j = 0; j < length; j++)
            {
                map[i, j] = 0;
                canBeChanged[i, j] = false;
                reversed[i, j] = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ClearMap();
            GenerateMap();
        }

        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform != null && hit.transform.gameObject != null)
                    {
                        int x = (int)(hit.transform.parent.position.z / distance);
                        int y = (int)(hit.transform.parent.position.x / distance);
                        //Debug.Log(canBeChanged[y,x] + " " + reversed[y, x]);
                        if (canBeChanged[y, x])
                        {
                            SwitchTracks(x, y);
                        }
                    }
                }
            }
        }
    }

    private void SwitchTracks(int x, int y)
    {
        if(map[y,x] == 2)
        {
            if (reversed[y, x])
            {
                if(y > 0 && canBeChanged[y - 1,x] && map[y - 1,x] == 2)
                {
                    Debug.Log("Bottom pressed");
                    map[y, x]--;
                    map[y - 1, x]--;
                    reversed[y, x] = false;
                    reversed[y - 1, x] = false;
                    tracks[(y * length + x) - length].Rotate(0, 180, 0);
                    ChechTrack(tracks[(y * length + x) - length], x, y - 1);
                    ChechTrack(tracks[(y * length + x)], x, y);
                }

                else if(y < lines - 1 && canBeChanged[y + 1, x] && map[y + 1, x] == 2)
                {
                    Debug.Log("Top pressed");
                    map[y, x]--;
                    map[y + 1, x]--;
                    reversed[y, x] = false;
                    reversed[y + 1, x] = false;
                    tracks[(y * length + x)].Rotate(0, 180, 0);
                    ChechTrack(tracks[(y * length + x) + length], x, y + 1);
                    ChechTrack(tracks[(y * length + x)], x, y);
                }
            }

            else
            {
                if (y > 0 && canBeChanged[y - 1, x] && map[y - 1, x] == 2)
                {
                    Debug.Log("Bottom pressed");
                    map[y, x]++;
                    map[y - 1, x]++;
                    reversed[y, x] = true;
                    reversed[y - 1, x] = true;
                    tracks[(y * length + x)].Rotate(0, 180, 0);
                    ChechTrack(tracks[(y * length + x) - length], x, y - 1);
                    ChechTrack(tracks[(y * length + x)], x, y);
                }

                else if (y < lines - 1 && canBeChanged[y + 1, x] && map[y + 1, x] == 2)
                {
                    Debug.Log("Top pressed");
                    map[y, x]++;
                    map[y + 1, x]++;
                    reversed[y, x] = true;
                    reversed[y + 1, x] = true;
                    tracks[(y * length + x) + length].Rotate(0, 180, 0);
                    ChechTrack(tracks[(y * length + x) + length], x, y + 1);
                    ChechTrack(tracks[(y * length + x)], x, y);
                }
            }
        }

        
        else if(map[y,x] == 3)
        {
            if (y > 0 && canBeChanged[y - 1, x] && map[y - 1, x] == 3 &&
                tracks[(y * length + x) - length].eulerAngles.y == 0)
            {
                Debug.Log("Bottom pressed");
                map[y, x]--;
                map[y - 1, x]--;
                tracks[(y * length + x)].Rotate(0, -180, 0);
                ChechTrack(tracks[(y * length + x) - length], x, y - 1);
                ChechTrack(tracks[(y * length + x)], x, y);
            }

            else if (y < lines - 1 && canBeChanged[y + 1, x] && map[y + 1, x] == 3)
            {
                Debug.Log("Top pressed");
                map[y, x]--;
                map[y + 1, x]--;
                tracks[(y * length + x) + length].Rotate(0, -180, 0);
                ChechTrack(tracks[(y * length + x) + length], x, y + 1);
                ChechTrack(tracks[(y * length + x)], x, y);
            }
        }

        else
        {
            if (y > 0 && canBeChanged[y - 1, x] && map[y - 1, x] == 1 &&
                tracks[(y * length + x) - length].eulerAngles.y == 180)
            {
                Debug.Log("Bottom pressed");
                map[y, x]++;
                map[y - 1, x]++;
                tracks[(y * length + x) - length].Rotate(0, -180, 0);
                ChechTrack(tracks[(y * length + x) - length], x, y - 1);
                ChechTrack(tracks[(y * length + x)], x, y);
            }

            else if (y < lines - 1 && canBeChanged[y + 1, x] && map[y + 1, x] == 1)
            {
                Debug.Log("Top pressed");
                map[y, x]++;
                map[y + 1, x]++;
                tracks[(y * length + x)].Rotate(0, -180, 0);
                ChechTrack(tracks[(y * length + x) + length], x, y + 1);
                ChechTrack(tracks[(y * length + x)], x, y);
            }
        }
    }

    private void ToOpaqueMode(Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    private void ToFadeMode(Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }

}
