using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BookshelfManager : MonoBehaviour
{
    public RectTransform[] slots;
    public GameObject bookPrefab;

    public TextMeshProUGUI hitsText;
    public TextMeshProUGUI faultsText;
    public TextMeshProUGUI currentPageText;

    public TMP_Dropdown algorithmDropdown;
    public Button startSimulationButton;

    private Queue<int> fifoQueue = new Queue<int>();
    private LinkedList<int> lruQueue = new LinkedList<int>();
    private GameObject[] activeBooks;
    private List<int> pageAccessSequence = new List<int> { 1, 2, 3, 4, 1, 2, 5, 1, 2, 3 }; // Example sequence

    private int pageHits = 0;
    private int pageFaults = 0;
    private int currentPageIndex = 0;
    private Coroutine simulationCoroutine;

    private void Start()
    {
        activeBooks = new GameObject[slots.Length];

        startSimulationButton.onClick.AddListener(OnStartSimulationClicked);
        algorithmDropdown.onValueChanged.AddListener(OnAlgorithmChanged);

        UpdatePageStats();
    }

    private void OnStartSimulationClicked()
    {
        if (simulationCoroutine != null)
            StopCoroutine(simulationCoroutine);

        simulationCoroutine = StartCoroutine(SimulatePageAccess());
    }

    private IEnumerator SimulatePageAccess()
    {
        for (currentPageIndex = 0; currentPageIndex < pageAccessSequence.Count; currentPageIndex++)
        {
            int page = pageAccessSequence[currentPageIndex];
            AccessPage(page);
            yield return new WaitForSeconds(1f);
        }

        currentPageText.text = "Simulation Complete";
    }

    private void OnAlgorithmChanged(int value)
    {
        if (simulationCoroutine != null)
            StopCoroutine(simulationCoroutine);

        pageHits = 0;
        pageFaults = 0;
        currentPageIndex = 0;

        ClearShelf();

        fifoQueue.Clear();
        lruQueue.Clear();

        UpdatePageStats();
    }

    public void AccessPage(int pageNumber)
    {
        string selectedAlgorithm = algorithmDropdown.options[algorithmDropdown.value].text;

        switch (selectedAlgorithm)
        {
            case "FIFO":
                ExecuteFIFO(pageNumber);
                break;
            case "LRU":
                ExecuteLRU(pageNumber);
                break;
            case "Optimal":
                ExecuteOptimal(pageNumber);
                break;
        }

        UpdatePageStats();
    }

    private void ExecuteFIFO(int pageNumber)
    {
        if (CheckIfPageExists(pageNumber))
        {
            pageHits++;
            UpdateBookColor(pageNumber, true);
        }
        else
        {
            pageFaults++;
            AddOrReplaceBook(pageNumber, fifoQueue);
        }
    }

    private void ExecuteLRU(int pageNumber)
    {
        if (CheckIfPageExists(pageNumber))
        {
            pageHits++;
            UpdateBookColor(pageNumber, true);
            lruQueue.Remove(pageNumber);
            lruQueue.AddFirst(pageNumber);
        }
        else
        {
            pageFaults++;
            AddOrReplaceBook(pageNumber, lruQueue);
        }
    }

    private void ExecuteOptimal(int pageNumber)
    {
        if (CheckIfPageExists(pageNumber))
        {
            pageHits++;
            UpdateBookColor(pageNumber, true);
        }
        else
        {
            pageFaults++;
            int pageToReplace = PredictOptimalPage();
            int slotToReplace = GetSlotIndexForPage(pageToReplace);
            RemoveBookFromSlot(slotToReplace);

            AddBookToSlot(slotToReplace, pageNumber);
        }
    }

    private int PredictOptimalPage()
    {
        Dictionary<int, int> futureIndex = new Dictionary<int, int>();

        foreach (var book in fifoQueue)
        {
            int nextUseIndex = pageAccessSequence.FindIndex(currentPageIndex + 1, x => x == book);
            futureIndex[book] = nextUseIndex == -1 ? int.MaxValue : nextUseIndex;
        }

        int farthestPage = -1;
        int farthestIndex = -1;

        foreach (var entry in futureIndex)
        {
            if (entry.Value > farthestIndex)
            {
                farthestIndex = entry.Value;
                farthestPage = entry.Key;
            }
        }

        fifoQueue.Dequeue(); // Remove from queue
        return farthestPage;
    }

    private void AddOrReplaceBook(int pageNumber, object queue)
    {
        if (queue is Queue<int> fifo)
        {
            if (fifo.Count < slots.Length)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (activeBooks[i] == null)
                    {
                        AddBookToSlot(i, pageNumber);
                        fifo.Enqueue(pageNumber);
                        return;
                    }
                }
            }
            else
            {
                int pageToReplace = fifo.Dequeue();
                int slotToReplace = GetSlotIndexForPage(pageToReplace);
                RemoveBookFromSlot(slotToReplace);

                AddBookToSlot(slotToReplace, pageNumber);
                fifo.Enqueue(pageNumber);
            }
        }
        else if (queue is LinkedList<int> lru)
        {
            if (lru.Count < slots.Length)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (activeBooks[i] == null)
                    {
                        AddBookToSlot(i, pageNumber);
                        lru.AddFirst(pageNumber);
                        return;
                    }
                }
            }
            else
            {
                int pageToReplace = lru.Last.Value;
                lru.RemoveLast();

                int slotToReplace = GetSlotIndexForPage(pageToReplace);
                RemoveBookFromSlot(slotToReplace);

                AddBookToSlot(slotToReplace, pageNumber);
                lru.AddFirst(pageNumber);
            }
        }
    }

    private bool CheckIfPageExists(int pageNumber)
    {
        foreach (var book in activeBooks)
        {
            if (book != null && book.name == "Book_" + pageNumber)
            {
                return true;
            }
        }
        return false;
    }

    private void AddBookToSlot(int slotIndex, int pageNumber)
    {
        GameObject newBook = Instantiate(bookPrefab, slots[slotIndex].position, Quaternion.identity, slots[slotIndex]);
        newBook.name = "Book_" + pageNumber;
        activeBooks[slotIndex] = newBook;
        newBook.GetComponent<Image>().color = Color.red;
    }

    private void RemoveBookFromSlot(int slotIndex)
    {
        Destroy(activeBooks[slotIndex]);
        activeBooks[slotIndex] = null;
    }

    private void UpdateBookColor(int pageNumber, bool isHit)
    {
        foreach (var book in activeBooks)
        {
            if (book != null && book.name == "Book_" + pageNumber)
            {
                book.GetComponent<Image>().color = isHit ? Color.green : Color.red;
                break;
            }
        }
    }

    private void UpdatePageStats()
    {
        hitsText.text = $"Page hits: {pageHits}";
        faultsText.text = $"Page faults: {pageFaults}";

        if (currentPageIndex < pageAccessSequence.Count)
        {
            currentPageText.text = $"Current Page: {pageAccessSequence[currentPageIndex]}";
        }
        else
        {
            currentPageText.text = "Simulation Complete";
        }
    }

    private int GetSlotIndexForPage(int pageNumber)
    {
        for (int i = 0; i < activeBooks.Length; i++)
        {
            if (activeBooks[i] != null && activeBooks[i].name == "Book_" + pageNumber)
            {
                return i;
            }
        }
        return -1;
    }

    private void ClearShelf()
    {
        foreach (var book in activeBooks)
        {
            if (book != null)
            {
                Destroy(book);
            }
        }
        activeBooks = new GameObject[slots.Length];
    }
}
