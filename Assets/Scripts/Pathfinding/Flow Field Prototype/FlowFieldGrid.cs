using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Post
{
    private int id;
    public Vector3 location;
    public Transform transform;
    public Vector3 volume;
    public Vector3 faceDirection;


    public Post() { }
    public Post(Transform location, Vector3 volume)
    {
        transform = location;
        this.location = transform.position;
        this.volume = volume;
    }


    


}

public class FlowFieldGrid : MonoBehaviour
{

    [SerializeField, Range(0.0f, 100.0f)] private float length = 50.0f;
    [SerializeField, Range(0, 100)] private int postCount = 5;
    public List<Vector3> postBoundary = new List<Vector3>();
    public List<Post> posts = new List<Post>();


    private int initialCount = 0;
    private float initialLength = 0.0f;

    public Transform target;
  

    [SerializeField] private bool drawPostBoundary = true;
    [SerializeField] private bool drawPosts = true;
    [SerializeField] private Color drawVolumeColour = Color.blue;
    [SerializeField] private bool drawForward = true;
    [SerializeField, Range(0, 5)] private float forwardLength = 2.0f;

    // Start is called before the first frame update
    void Awake()
    {
        GeneratePoints();
    }

    // Update is called once per frame
    void Update()
    {
        if(initialCount != postCount || initialLength != length)
        {
            GeneratePoints();
        }


        foreach(Post post in posts)
        {
            post.transform.LookAt(target);
        }


    }

    private void GeneratePoints()
    {
        postBoundary.Clear();
        posts.Clear();

        Vector3 initPos = transform.position - new Vector3(length * 0.5f, 0.0f, length * 0.5f);
        float lengthPerPost = length / postCount;

        for(int i = 0;  i < postCount + 1; i++)
        {
            for (int j = 0; j < postCount + 1; j++)
            {
                Vector3 newPostBoundary = initPos + new Vector3(i * lengthPerPost, 0.0f, j * lengthPerPost);
                postBoundary.Add(newPostBoundary);
            }
        }

        GameObject obj = new GameObject("Field Grid");
        obj.transform.parent = transform;

        for (float i = 0.5f; i < postCount; i++)
        {
            for (float j = 0.5f; j < postCount; j++)
            {
                Vector3 newPost = initPos + new Vector3(i * lengthPerPost, 0.0f, j * lengthPerPost);
                Vector3 volume = new Vector3(length / postCount, 0.0f, length / postCount);
                //Transform newTransform = obj.transform;
                //newTransform.position = newPost;  

                string postName = "Post " + (int)i + "_" + (int)j;
                GameObject postObj = new GameObject(postName);
                postObj.transform.parent = obj.transform;
                postObj.transform.position = newPost;

                posts.Add(new Post(postObj.transform, volume));
            }
        }

        initialCount = postCount;
        initialLength = length;

    }


    public bool GetFlowDir(Vector3 currentPos, out Vector3 dir)
    {
        dir = Vector3.zero;

        foreach(Post post in posts)
        {
            if(Mathf.Abs(Vector3.Distance(post.transform.position, currentPos)) < (length / postCount) * 0.8f)
            {
                dir = post.transform.forward;
                return true;
            }
        }
        
        return false;
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(length, 0.0f, length));
        
        //Center
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.yellow;

        if(postBoundary.Count > 0 && drawPostBoundary)
        {
            foreach(var post in postBoundary)
            {
                Gizmos.DrawSphere(post, 0.2f);

            }
        }


        if(posts.Count > 0 && drawPosts)
        {
            foreach (Post post in posts)
            {
                print(post.location);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(post.location, 0.2f);


                //easy draw cube volume for post
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(post.location, post.volume);

                Gizmos.color = drawVolumeColour;
                Gizmos.DrawCube(post.location, post.volume);
            }


            if(drawForward)
            {
                Gizmos.color = Color.black;

                foreach(Post post in posts)
                {
                    Gizmos.DrawLine(post.location, post.location + post.transform.forward * forwardLength);
                }
            }

        }




    }
}
