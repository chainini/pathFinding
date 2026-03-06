using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Ma_UIGame : MonoBehaviour, IPointerDownHandler
{
    public int row;
    public int column;

    public RectTransform rect;
    public GameObject prefab;

    List<List<int>> mathch = new List<List<int>>();
    List<List<Ma_MatchItem>> list = new List<List<Ma_MatchItem>>();

    public Ma_MatchItem selectMatchItem;

    void Start()
    {
        Init();
    }


    public float diffPosY;
    public float diffPosX;

    public void Init()
    {
        for (int i = 0; i < column; i++)
        {
            var rowList = new List<int>();
            for (int j = 0; j < row; j++)
            {
                rowList.Add(Random.Range(1, 6));
            }

            mathch.Add(rowList);
        }

        //���mathch�Ƿ��п���������  �������������
        for (int i = 0; i < column; i++)
        {
            var rowList = new List<Ma_MatchItem>();
            for (int j = 0; j < row; j++)
            {
                Ma_MatchItem matchItem = Instantiate(prefab, transform).GetComponent<Ma_MatchItem>();
                matchItem.SetData(new Vector2(i, j), mathch[i][j]);
                rowList.Add(matchItem);
            }

            list.Add(rowList);
        }


        //GetComponent<GridLayoutGroup>().enabled = false;
    }

    List<(int, int)> bfs(int startX, int startY)
    {
        var island = new List<(int, int)>();
        var queue = new Queue<(int, int)>();
        var visited = new HashSet<(int, int)>();

        int target = mathch[startX][startY];
        queue.Enqueue((startX, startY));
        visited.Add((startX, startY));

        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            island.Add((x, y));

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];

                if (nx >= 0 && nx < mathch.Count && ny >= 0 && ny < mathch[0].Count &&
                    !visited.Contains((nx, ny)) && mathch[nx][ny] == target)
                {
                    visited.Add((nx, ny));
                    queue.Enqueue((nx, ny));
                }
            }
        }

        return island;
    }

    bool CheckHasMathch(List<List<int>> mathch)
    {
        int rows = mathch.Count;
        int cols = mathch[0].Count;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var island = bfs(i, j);
                if (island.Count >= 3)
                    return true;
            }
        }

        return false;
    }


    HashSet<(int, int)> animatingCells = new HashSet<(int, int)>();

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect, // ��� RectTransform
            eventData.position, // ���/������Ļ����
            eventData.pressEventCamera, // ��Ӧ�� Camera��Screen Space - Camera��; Overlay �ɴ� null
            out localPoint
        );

        Debug.Log($"screen: {eventData.position}, local: {localPoint}");

        int x = Mathf.FloorToInt((rect.sizeDelta.y / 2 - localPoint.y) / 106);
        int y = Mathf.FloorToInt((rect.sizeDelta.x / 2 + localPoint.x) / 106);

        Debug.Log($"rect: {rect.sizeDelta}, x: {x}, y: {y}");

        if (x < 0 || x >= column || y < 0 || y >= row) return;


        var cell = (x, y);
        if (animatingCells.Contains(cell)) return;


        if (selectMatchItem == null)
        {
            selectMatchItem = list[x][y];
        }
        else if (selectMatchItem.myPos != list[x][y].myPos)
        {
            int sx = (int)selectMatchItem.myPos.x;
            int sy = (int)selectMatchItem.myPos.y;

            if (animatingCells.Contains((sx, sy)) || animatingCells.Contains(cell)) return;

            swaperMatchItem(selectMatchItem, list[x][y]);
            selectMatchItem = null;
        }
        else
        {
            selectMatchItem = list[x][y];
        }
    }

    public void swaperMatchItem(Ma_MatchItem matchItem, Ma_MatchItem matchItem2, bool needcheck = true)
    {
        int x = (int)matchItem.myPos.x;
        int y = (int)matchItem.myPos.y;
        int x2 = (int)matchItem2.myPos.x;
        int y2 = (int)matchItem2.myPos.y;

        animatingCells.Add((x, y));
        animatingCells.Add((x2, y2));


        var layout = gameObject.GetComponent<GridLayoutGroup>();
        layout.enabled = false;

        diffPosX = Mathf.Abs(list[1][1].transform.position.x - list[0][0].transform.position.x);
        diffPosY = Mathf.Abs(list[1][1].transform.position.y - list[0][0].transform.position.y);
        Debug.Log($"diffPosX:{diffPosX},diffPosY:{diffPosY}");

        var pos = matchItem.gameObject.transform.position;
        var pos2 = matchItem2.gameObject.transform.position;

        var seq = DOTween.Sequence();
        seq.Insert(0, matchItem.transform.DOMove(pos2, 0.2f));
        seq.Insert(0, matchItem.transform.DOScale(Vector3.one * 0.2f, 0.1f));
        seq.Insert(0.1f, matchItem.transform.DOScale(Vector3.one, 0.1f));


        seq.Insert(0, matchItem2.transform.DOMove(pos, 0.2f));
        seq.Insert(0, matchItem2.transform.DOScale(Vector3.one * 0.2f, 0.1f));
        seq.Insert(0.1f, matchItem2.transform.DOScale(Vector3.one, 0.1f));

        //if (!needcheck) return;

        seq.OnComplete(() =>
        {
            //����


            mathch[x][y] = matchItem2.myType;
            mathch[x2][y2] = matchItem.myType;

            list[x][y] = matchItem2;
            list[x2][y2] = matchItem;

            var tempPos = matchItem.myPos;
            matchItem.myPos = matchItem2.myPos;
            matchItem2.myPos = tempPos;

            Debug.Log($"CheckCanMatch: {CheckCanMatch(x, y, x2, y2)}");

            if (CheckCanMatch(x, y, x2, y2))
            {
                animatingCells.Remove((x, y));
                animatingCells.Remove((x2, y2));
                Match();
            }
            else
            {
                PlayReverseAnimation(matchItem2, matchItem);
            }
        });
    }

    public void Match()
    {
        //动画
        var seq = DOTween.Sequence();
        for (int i = 0; i < land.Count; i++)
        {
            seq.Insert(0, list[land[i].Item1][land[i].Item2].transform.DOScale(Vector3.zero, 0.2f));
        }

        seq.OnComplete(() =>
        {
            // 按列收集被消除的对象
            var recycledByCol = new Dictionary<int, List<Ma_MatchItem>>();

            for (int i = 0; i < land.Count; i++)
            {
                int x = land[i].Item1;
                int y = land[i].Item2;
                mathch[x][y] = 0;

                if (!recycledByCol.ContainsKey(y))
                {
                    recycledByCol[y] = new List<Ma_MatchItem>();
                }

                recycledByCol[y].Add(list[x][y]);
            }

            var fallSeq = DOTween.Sequence();
            //下落
            for (int j = 0; j < row; j++)
            {
                int writePos = column - 1;
                for (int i = column - 1; i >= 0; i--)
                {
                    if (mathch[i][j] != 0)
                    {
                        if (i != writePos)
                        {
                            var targetPos = list[writePos][j].transform.position;
                            mathch[writePos][j] = mathch[i][j];
                            list[writePos][j] = list[i][j];
                            list[writePos][j].myPos = new Vector2(writePos, j);
                            fallSeq.Insert(0, list[writePos][j].transform.DOMove(targetPos, 0.3f));
                        }

                        writePos--;
                    }
                }

                var topPos = list[0][j].transform.position;
                int bitOffset = 1;
                int recycledIndex = 0;
                for (int i = writePos; i >= 0; i--)
                {
                    var item = recycledByCol[j][recycledIndex++];
                    var targetPos = list[i][j].transform.position;

                    int newType = Random.Range(1, 6);
                    mathch[i][j] = newType;
                    list[i][j] = item;
                    item.transform.localScale = Vector3.one;
                    item.SetData(new Vector2(i, j), newType);

                    item.transform.position = topPos + new Vector3(0, Mathf.Abs(diffPosY) * bitOffset, 0);
                    fallSeq.Insert(0.15f, item.transform.DOMove(targetPos, 0.3f));
                    bitOffset++;
                }
            }
            
        });
    }

    List<(int, int)> land;

    private bool CheckCanMatch(int x, int y, int x2, int y2)
    {
        var island = bfs(x, y);
        var island2 = bfs(x2, y2);
        Debug.Log($"island: {island.Count}, island2: {island2.Count}");
        if (island.Count >= 3 && island2.Count >= 3)
        {
            land = island.Union(island2).ToList();
        }
        else if (island.Count >= 3)
        {
            land = island.ToList();
        }
        else if (island2.Count >= 3)
        {
            land = island2.ToList();
        }

        return island.Count >= 3 || island2.Count >= 3;
    }

    void PlayReverseAnimation(Ma_MatchItem item1, Ma_MatchItem item2)
    {
        var pos = item1.gameObject.transform.position;
        var pos2 = item2.gameObject.transform.position;

        var reverseSeq = DOTween.Sequence();
        reverseSeq.Insert(0, item1.transform.DOMove(pos2, 0.2f));
        reverseSeq.Insert(0, item1.transform.DOScale(Vector3.one * 0.2f, 0.1f));
        reverseSeq.Insert(0.1f, item1.transform.DOScale(Vector3.one, 0.1f));

        reverseSeq.Insert(0, item2.transform.DOMove(pos, 0.2f));
        reverseSeq.Insert(0, item2.transform.DOScale(Vector3.one * 0.2f, 0.1f));
        reverseSeq.Insert(0.1f, item2.transform.DOScale(Vector3.one, 0.1f));

        reverseSeq.OnComplete(() =>
        {
            // ������ɣ��ָ�����
            int x = (int)item1.myPos.x;
            int y = (int)item1.myPos.y;
            int x2 = (int)item2.myPos.x;
            int y2 = (int)item2.myPos.y;

            mathch[x][y] = item2.myType;
            mathch[x2][y2] = item1.myType;

            list[x][y] = item2;
            list[x2][y2] = item1;

            var tempPos = item1.myPos;
            item1.myPos = item2.myPos;
            item2.myPos = tempPos;

            animatingCells.Remove((x, y));
            animatingCells.Remove((x2, y2));
        });
    }
}