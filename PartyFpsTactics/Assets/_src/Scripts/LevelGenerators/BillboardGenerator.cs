using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Health;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BillboardGenerator : MonoBehaviour
{
    public string currentBillboardSign = "welcome to dust";
     Vector3Int BillboardDirection = Vector3Int.left;
    public Vector3 lettersEuler = Vector3.zero;
     Vector3 BillboardStartPos = Vector3.zero;
    public int wallSize = 10;
    public int lettersSpacingInTilesHorizontal = 3;
    public int lettersSpacingInTilesVertical  = 2;
    public List<BillboardLetter> letters;
    public List<string> wordsToShuffle;

    public List<TileHealth> spawnedLetters = new List<TileHealth>();

    [ContextMenu("GenerateBillboard")]
    public void GenerateBillboard()
    {
        GenerateBillboard(wallSize,Vector3.zero, 270);
    }
    public void GenerateBillboard(int _wallSize, Vector3 _position, float yRot)
    {
        currentBillboardSign = String.Empty;
        for (int i = spawnedLetters.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(spawnedLetters[i].gameObject);    
        }
        spawnedLetters.Clear();
        var temp = new List<string>(wordsToShuffle);
        for (int i = 0; i < Random.Range(2, 4); i++)
        {
            int r = Random.Range(0, temp.Count);

            currentBillboardSign += temp[r] + " ";
            temp.RemoveAt(r);
        }  
        
        string sign = currentBillboardSign.ToUpper();
        int xxx = 0;
        int rows = 0;
        int x = 0;
        BillboardStartPos.x = _wallSize / 2;
        for (int i = 0; i < sign.Length; i++)
        {
            TileHealth letterPrefab = null;
            foreach (var billboardLetter in letters)
            {
                if (billboardLetter.letter.ToUpper()[0] == sign[i])
                {
                    letterPrefab = billboardLetter.letterPrefab;
                    break;
                }
            }

            xxx += lettersSpacingInTilesHorizontal;
            
            if (xxx >= _wallSize - 2) // new row
            {
                rows++;
                xxx = 0;
                x = 0;
                foreach (var spawnedLetter in spawnedLetters)
                {
                    spawnedLetter.transform.position += Vector3.up * lettersSpacingInTilesVertical;   
                }
            }

            x++;
            if (letterPrefab == null)
                continue;
            
            var letter = Instantiate(letterPrefab,
                transform.position + BillboardStartPos + BillboardDirection * lettersSpacingInTilesHorizontal * x + Vector3.down * lettersSpacingInTilesVertical * rows,
                Quaternion.identity);
            letter.transform.parent = transform;
            letter.transform.localRotation = Quaternion.Euler(lettersEuler);
            spawnedLetters.Add(letter);
        }

        transform.position = _position;
        switch (rows)
        {
            case 1:
                transform.localScale = Vector3.one * 0.75f;
                break;
            case 2:
                transform.localScale = Vector3.one * 0.5f;
                break;
            case 3:
                transform.localScale = Vector3.one * 0.33f;
                break;
        } 
        transform.eulerAngles = new Vector3(0, yRot, 0);
        StartCoroutine(MakeLettersDependentOnTiles());
    }

    IEnumerator MakeLettersDependentOnTiles()
    {
        for (int i = 0; i < spawnedLetters.Count; i++)
        {
            var letter = spawnedLetters[i];
            var colliders = Physics.SphereCastAll(letter.transform.position + transform.forward, 0.5f,
                -transform.forward, 2, GameManager.Instance.AllSolidsMask);
            for (int j = 0; j < colliders.Length; j++)
            {
                if (colliders[j].transform == letter.transform)
                    continue;

                var supporterTile = colliders[j].collider.gameObject.GetComponent<TileHealth>();
                if (supporterTile)
                {
                    supporterTile.objectsToDestroyOnClash.Add(letter);
                    letter.supporterTile = supporterTile;
                    break;   
                }
            }
            yield return null;
        }
    }
    
    [Serializable]
    public class BillboardLetter
    {
        public string letter = "";
        public TileHealth letterPrefab;
    }
}
