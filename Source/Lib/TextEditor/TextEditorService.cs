using Microsoft.JSInterop;
using System;
using System.Text;
using Walk.Common.RazorLib;
using Walk.Common.RazorLib.BackgroundTasks.Models;
using Walk.Common.RazorLib.JsRuntimes.Models;
using Walk.Common.RazorLib.Keys.Models;
using Walk.Common.RazorLib.Menus.Models;
using Walk.Common.RazorLib.Reactives.Models;
using Walk.TextEditor.RazorLib.BackgroundTasks.Models;
using Walk.TextEditor.RazorLib.Groups.Models;
using Walk.TextEditor.RazorLib.Installations.Models;
using Walk.TextEditor.RazorLib.JsRuntimes.Models;
using Walk.TextEditor.RazorLib.Lexers.Models;
using Walk.TextEditor.RazorLib.Lines.Models;
using Walk.TextEditor.RazorLib.TextEditors.Models;

namespace Walk.TextEditor.RazorLib;

public sealed partial class TextEditorService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IServiceProvider _serviceProvider;

    public TextEditorService(
        WalkTextEditorConfig textEditorConfig,
        IJSRuntime jsRuntime,
        CommonService commonService,
        IServiceProvider serviceProvider)
    {
        __TextEditorViewModelLiason = new(this);
    
        WorkerUi = new(this);
        WorkerArbitrary = new(this);
    
        CommonService = commonService;
        
        PostScrollAndRemeasure_DebounceExtraEvent = new(
            TimeSpan.FromMilliseconds(250),
            CancellationToken.None,
            (_, _) =>
            {
                CommonService.AppDimension_NotifyIntraAppResize(useExtraEvent: false);
                return Task.CompletedTask;
            });
        
        _serviceProvider = serviceProvider;
        TextEditorConfig = textEditorConfig;
        _jsRuntime = jsRuntime;
        JsRuntimeTextEditorApi = _jsRuntime.GetWalkTextEditorApi();
        
        TextEditorState = new();
    }

    public CommonService CommonService { get; }

    public WalkTextEditorJavaScriptInteropApi JsRuntimeTextEditorApi { get; }
    public WalkCommonJavaScriptInteropApi JsRuntimeCommonApi => CommonService.JsRuntimeCommonApi;
    public WalkTextEditorConfig TextEditorConfig { get; }

#if DEBUG
    public string StorageKey => "di_te_text-editor-options-debug";
#else
    public string StorageKey => "di_te_text-editor-options";
#endif

    public string ThemeCssClassString { get; set; }
    
    public TextEditorState TextEditorState { get; }
    
    public TextEditorWorkerUi WorkerUi { get; }
    public TextEditorWorkerArbitrary WorkerArbitrary { get; }
    
    public object IdeBackgroundTaskApi { get; set; }
    
    /// <summary>
    /// Do not touch this property, it is used for the VirtualizationResult.
    /// </summary>
    public StringBuilder __StringBuilder { get; } = new StringBuilder();
    
    /// <summary>
    /// Do not touch this property, it is used for the ICompilerService implementations.
    /// </summary>
    public StringWalker __StringWalker { get; } = new StringWalker();
 
    /// <summary>
    /// Do not touch this property, it is used for the TextEditorEditContext.
    /// </summary>
    public List<TextEditorModel> __ModelList { get; } = new();   
    /// <summary>
    /// Do not touch this property, it is used for the TextEditorEditContext.
    /// </summary>
    public List<TextEditorViewModel> __ViewModelList { get; } = new();
    
    /// <summary>
    /// Do not touch this property, it is used for the 'TextEditorModel.InsertMetadata(...)' method.
    /// </summary>
    public List<LineEnd> __LocalLineEndList { get; } = new();
    /// <summary>
    /// Do not touch this property, it is used for the 'TextEditorModel.InsertMetadata(...)' method.
    /// </summary>
    public List<int> __LocalTabPositionList { get; } = new();
    /// <summary>
    /// Do not touch this property, it is used for the 'TextEditorModel.InsertMetadata(...)' method.
    /// </summary>
    public TextEditorViewModelLiason __TextEditorViewModelLiason { get; }
    
    /// <summary>
    /// Do not touch this property, it is used for the 'TextEditorVirtualizationResult.PresentationVirtualizeAndShiftTextSpans(...)' method.
    /// </summary>
    public List<TextEditorTextSpan> __VirtualizedTextSpanList { get; set; } = new();
    /// <summary>
    /// Do not touch this property, it is used for the 'TextEditorVirtualizationResult.PresentationVirtualizeAndShiftTextSpans(...)' method.
    /// </summary>
    public List<TextEditorTextSpan> __OutTextSpansList { get; set; } = new();
    
    public int SeenTabWidth { get; set; }
    public string TabKeyOutput_ShowWhitespaceTrue { get; set; }
    public string TabKeyOutput_ShowWhitespaceFalse { get; set; }
    
    public int TabKeyBehavior_SeenTabWidth { get; set; }
    public string TabKeyBehavior_TabSpaces { get; set; }
    
    /// <summary>
    /// To avoid unexpected HTML movements when responding to a PostScrollAndRemeasure(...)
    /// this debounce will add 1 extra event after everything has "settled".
    ///
    /// `byte` is just a throwaway generic type, it isn't used.
    /// </summary>
    public Debounce<byte> PostScrollAndRemeasure_DebounceExtraEvent { get; }
    
    private readonly StringPoolBucket _bucket_123 = new();
    private readonly StringPoolBucket _bucket_456 = new();
    private readonly StringPoolBucket _bucket_789 = new();
    private readonly StringPoolBucket _bucket_101112 = new();
    private readonly StringPoolBucket _bucket_131415 = new();
    private readonly StringPoolBucket _bucket_default = new();
    
    public class StringPoolBucket
    {
        private readonly List<string> _stringList = new();

        public int Count => _stringList.Count;

        // Uppercase

        private int A;
        private int B;
        private int C;
        private int D;
        private int E;
        private int F;
        private int G;
        private int H;
        private int I;
        private int J;
        private int K;
        private int L;
        private int M;
        private int N;
        private int O;
        private int P;
        private int Q;
        private int R;
        private int S;
        private int T;
        private int U;
        private int V;
        private int W;
        private int X;
        private int Y;
        private int Z;
        // Lowercase
        private int a;
        private int b;
        private int c;
        private int d;
        private int e;
        private int f;
        private int g;
        private int h;
        private int i;
        private int j;
        private int k;
        private int l;
        private int m;
        private int n;
        private int o;
        private int p;
        private int q;
        private int r;
        private int s;
        private int t;
        private int u;
        private int v;
        private int w;
        private int x;
        private int y;
        private int z;
        // _
        private int _;
        // defaultGroup
        private int defaultGroup;
        
        /// <summary>
        /// span.Length of 0 doesn't get a bucket so span[0] is always safe.
        /// </summary>
        public string Get(ReadOnlySpan<char> span)
        {
            int startInclusiveIndex;
            int endExclusiveIndex;
        
            switch (span[0])
            {
                case 'A':
                    startInclusiveIndex = A;
                    endExclusiveIndex = B;
                    break;
                case 'B':
                    startInclusiveIndex = B;
                    endExclusiveIndex = C;
                    break;
                case 'C':
                    startInclusiveIndex = C;
                    endExclusiveIndex = D;
                    break;
                case 'D':
                    startInclusiveIndex = D;
                    endExclusiveIndex = E;
                    break;
                case 'E':
                    startInclusiveIndex = E;
                    endExclusiveIndex = F;
                    break;
                case 'F':
                    startInclusiveIndex = F;
                    endExclusiveIndex = G;
                    break;
                case 'G':
                    startInclusiveIndex = G;
                    endExclusiveIndex = H;
                    break;
                case 'H':
                    startInclusiveIndex = H;
                    endExclusiveIndex = I;
                    break;
                case 'I':
                    startInclusiveIndex = I;
                    endExclusiveIndex = J;
                    break;
                case 'J':
                    startInclusiveIndex = J;
                    endExclusiveIndex = K;
                    break;
                case 'K':
                    startInclusiveIndex = K;
                    endExclusiveIndex = L;
                    break;
                case 'L':
                    startInclusiveIndex = L;
                    endExclusiveIndex = M;
                    break;
                case 'M':
                    startInclusiveIndex = M;
                    endExclusiveIndex = N;
                    break;
                case 'N':
                    startInclusiveIndex = N;
                    endExclusiveIndex = O;
                    break;
                case 'O':
                    startInclusiveIndex = O;
                    endExclusiveIndex = P;
                    break;
                case 'P':
                    startInclusiveIndex = P;
                    endExclusiveIndex = Q;
                    break;
                case 'Q':
                    startInclusiveIndex = Q;
                    endExclusiveIndex = R;
                    break;
                case 'R':
                    startInclusiveIndex = R;
                    endExclusiveIndex = S;
                    break;
                case 'S':
                    startInclusiveIndex = S;
                    endExclusiveIndex = T;
                    break;
                case 'T':
                    startInclusiveIndex = T;
                    endExclusiveIndex = U;
                    break;
                case 'U':
                    startInclusiveIndex = U;
                    endExclusiveIndex = V;
                    break;
                case 'V':
                    startInclusiveIndex = V;
                    endExclusiveIndex = W;
                    break;
                case 'W':
                    startInclusiveIndex = W;
                    endExclusiveIndex = X;
                    break;
                case 'X':
                    startInclusiveIndex = X;
                    endExclusiveIndex = Y;
                    break;
                case 'Y':
                    startInclusiveIndex = Y;
                    endExclusiveIndex = Z;
                    break;
                case 'Z':
                    startInclusiveIndex = Z;
                    endExclusiveIndex = a;
                    break;
                case 'a':
                    startInclusiveIndex = a;
                    endExclusiveIndex = b;
                    break;
                case 'b':
                    startInclusiveIndex = b;
                    endExclusiveIndex = c;
                    break;
                case 'c':
                    startInclusiveIndex = c;
                    endExclusiveIndex = d;
                    break;
                case 'd':
                    startInclusiveIndex = d;
                    endExclusiveIndex = e;
                    break;
                case 'e':
                    startInclusiveIndex = e;
                    endExclusiveIndex = f;
                    break;
                case 'f':
                    startInclusiveIndex = f;
                    endExclusiveIndex = g;
                    break;
                case 'g':
                    startInclusiveIndex = g;
                    endExclusiveIndex = h;
                    break;
                case 'h':
                    startInclusiveIndex = h;
                    endExclusiveIndex = i;
                    break;
                case 'i':
                    startInclusiveIndex = i;
                    endExclusiveIndex = j;
                    break;
                case 'j':
                    startInclusiveIndex = j;
                    endExclusiveIndex = k;
                    break;
                case 'k':
                    startInclusiveIndex = k;
                    endExclusiveIndex = l;
                    break;
                case 'l':
                    startInclusiveIndex = l;
                    endExclusiveIndex = m;
                    break;
                case 'm':
                    startInclusiveIndex = m;
                    endExclusiveIndex = n;
                    break;
                case 'n':
                    startInclusiveIndex = n;
                    endExclusiveIndex = o;
                    break;
                case 'o':
                    startInclusiveIndex = o;
                    endExclusiveIndex = p;
                    break;
                case 'p':
                    startInclusiveIndex = p;
                    endExclusiveIndex = q;
                    break;
                case 'q':
                    startInclusiveIndex = q;
                    endExclusiveIndex = r;
                    break;
                case 'r':
                    startInclusiveIndex = r;
                    endExclusiveIndex = s;
                    break;
                case 's':
                    startInclusiveIndex = s;
                    endExclusiveIndex = t;
                    break;
                case 't':
                    startInclusiveIndex = t;
                    endExclusiveIndex = u;
                    break;
                case 'u':
                    startInclusiveIndex = u;
                    endExclusiveIndex = v;
                    break;
                case 'v':
                    startInclusiveIndex = v;
                    endExclusiveIndex = w;
                    break;
                case 'w':
                    startInclusiveIndex = w;
                    endExclusiveIndex = x;
                    break;
                case 'x':
                    startInclusiveIndex = x;
                    endExclusiveIndex = y;
                    break;
                case 'y':
                    startInclusiveIndex = y;
                    endExclusiveIndex = z;
                    break;
                case 'z':
                    startInclusiveIndex = z;
                    endExclusiveIndex = _;
                    break;
                case '_':
                    startInclusiveIndex = _;
                    endExclusiveIndex = defaultGroup;
                    break;
                default:
                    startInclusiveIndex = defaultGroup;
                    endExclusiveIndex = _stringList.Count;
                    break;
            }
            
            for (int i = startInclusiveIndex; i < endExclusiveIndex; i++)
            {
                if (span.SequenceEqual(_stringList[i]))
                    return _stringList[i];
            }
            
            // Allocate the new string and update all the indices.
            //
            // Insert at the end of the first letter group
            // presuming this on average results in more often used strings
            // being at the front of the first letter group.

            _stringList.Insert(endExclusiveIndex, span.ToString());            switch (span[0])
            {
                case 'A':
                    B++;
                    C++;
                    D++;
                    E++;
                    F++;
                    G++;
                    H++;
                    I++;
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'B':
                    C++;
                    E++;
                    E++;
                    F++;
                    G++;
                    H++;
                    I++;
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'C':
                    D++;
                    E++;
                    F++;
                    G++;
                    H++;
                    I++;
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'D':
                    E++;
                    F++;
                    G++;
                    H++;
                    I++;
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'E':
                    F++;
                    G++;
                    H++;
                    I++;
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'F':
                    G++;
                    H++;
                    I++;
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'G':
                    H++;
                    I++;
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'H':
                    I++;
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'I':
                    J++;
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'J':
                    K++;
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'K':
                    L++;
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'L':
                    M++;
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'M':
                    N++;
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'N':
                    O++;
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'O':
                    P++;
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'P':
                    Q++;
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'Q':
                    R++;
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'R':
                    S++;
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'S':
                    T++;
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'T':
                    U++;
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'U':
                    V++;
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'V':
                    W++;
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'W':
                    X++;
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'X':
                    Y++;
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'Y':
                    Z++;
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'Z':
                    a++;
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                // Lowercase
                case 'a':
                    b++;
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'b':
                    c++;
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'c':
                    d++;
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'd':
                    e++;
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'e':
                    f++;
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'f':
                    g++;
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'g':
                    h++;
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'h':
                    i++;
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'i':
                    j++;
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'j':
                    k++;
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'k':
                    l++;
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'l':
                    m++;
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'm':
                    n++;
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'n':
                    o++;
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'o':
                    p++;
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'p':
                    q++;
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'q':
                    r++;
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'r':
                    s++;
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 's':
                    t++;
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 't':
                    u++;
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'u':
                    v++;
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'v':
                    w++;
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'w':
                    x++;
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'x':
                    y++;
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'y':
                    z++;
                    _++;
                    defaultGroup++;
                    break;
                case 'z':
                    _++;
                    defaultGroup++;
                    break;
                case '_':
                    defaultGroup++;
                    break;
                default:                    break;
            }
            
            return _stringList[endExclusiveIndex];
        }

        public void Clear()        {            _stringList.Clear();
            
            // Uppercase
            A = 0;
            B = 0;
            C = 0;
            D = 0;
            E = 0;
            F = 0;
            G = 0;
            H = 0;
            I = 0;
            J = 0;
            K = 0;
            L = 0;
            M = 0;
            N = 0;
            O = 0;
            P = 0;
            Q = 0;
            R = 0;
            S = 0;
            T = 0;
            U = 0;
            V = 0;
            W = 0;
            X = 0;
            Y = 0;
            Z = 0;
            // Lowercase
            a = 0;
            b = 0;
            c = 0;
            d = 0;
            e = 0;
            f = 0;
            g = 0;
            h = 0;
            i = 0;
            j = 0;
            k = 0;
            l = 0;
            m = 0;
            n = 0;
            o = 0;
            p = 0;
            q = 0;
            r = 0;
            s = 0;
            t = 0;
            u = 0;
            v = 0;
            w = 0;
            x = 0;
            y = 0;
            z = 0;
            // _
            _ = 0;
            // defaultGroup
            defaultGroup = 0;        }
    }
    
    /// <summary>
    /// Provides string pooling for the ICompilerService implementations.
    ///
    /// This is only safe to use if you're in a TextEditorEditContext.
    /// (i.e.: there is an instance of TextEditorEditContext in scope)
    /// </summary>
    public string EditContext_GetText(ReadOnlySpan<char> span)
    {
        // Group by span length
        //
        // Each bucket is alphabetically sorted (only by the first letter)
        //
        // Track index of each first letter
        // 
        // Linear search within the first letter
    
        switch (span.Length)
        {
            case 0:
                return string.Empty;
            case 1:
            case 2:
            case 3:
                return _bucket_123.Get(span);
            case 4:
            case 5:
            case 6:
                return _bucket_456.Get(span);
            case 7:
            case 8:
            case 9:
                return _bucket_789.Get(span);
            case 10:
            case 11:
            case 12:
                return _bucket_101112.Get(span);
            case 13:
            case 14:
            case 15:
                return _bucket_131415.Get(span);
            default:
                return _bucket_default.Get(span);
        }
    }

    public void EditContext_GetText_Clear()
    {        _bucket_123.Clear();        _bucket_456.Clear();        _bucket_789.Clear();        _bucket_101112.Clear();        _bucket_131415.Clear();        _bucket_default.Clear();
    }
    
    public int EditContext_GetText_Count()
    {        return _bucket_123.Count +            _bucket_456.Count +            _bucket_789.Count +            _bucket_101112.Count +            _bucket_131415.Count +            _bucket_default.Count;
    }

    public void InsertTab(TextEditorEditContext editContext, TextEditorModel modelModifier, TextEditorViewModel viewModel)
    {
        if (Options_GetOptions().TabKeyBehavior)
        {
            modelModifier.Insert(
                "\t",
                viewModel);
        }
        else
        {
            if (TabKeyBehavior_SeenTabWidth != Options_GetOptions().TabWidth)
            {
                TabKeyBehavior_SeenTabWidth = Options_GetOptions().TabWidth;
                TabKeyBehavior_TabSpaces = new string(' ', TabKeyBehavior_SeenTabWidth);
            }
            modelModifier.Insert(
                TabKeyBehavior_TabSpaces,
                viewModel);
        }
    }

    public async ValueTask FinalizePost(TextEditorEditContext editContext)
    {
        for (int modelIndex = 0; modelIndex < __ModelList.Count; modelIndex++)
        {
            var modelModifier = __ModelList[modelIndex];
            
            for (int viewModelIndex = 0; viewModelIndex < modelModifier.PersistentState.ViewModelKeyList.Count; viewModelIndex++)
            {
                // Invoking 'GetViewModelModifier' marks the view model to be updated.
                var viewModelModifier = editContext.GetViewModelModifier(modelModifier.PersistentState.ViewModelKeyList[viewModelIndex]);

                if (!viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult)
                    viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = modelModifier.ShouldCalculateVirtualizationResult;
            }

            if (modelModifier.WasDirty != modelModifier.IsDirty)
            {
                var model = Model_GetOrDefault(modelModifier.PersistentState.ResourceUri);
                model.IsDirty = modelModifier.IsDirty;
            
                if (modelModifier.IsDirty)
                    AddDirtyResourceUri(modelModifier.PersistentState.ResourceUri);
                else
                    RemoveDirtyResourceUri(modelModifier.PersistentState.ResourceUri);
            }
            
            TextEditorState._modelMap[modelModifier.PersistentState.ResourceUri] = modelModifier;
        }
        
        for (int viewModelIndex = 0; viewModelIndex < __ViewModelList.Count; viewModelIndex++)
        {
            var viewModelModifier = __ViewModelList[viewModelIndex];
        
            TextEditorModel? modelModifier = null;
            if (viewModelModifier.PersistentState.ShouldRevealCursor || viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult || viewModelModifier.ScrollWasModified || viewModelModifier.PersistentState.Changed_Cursor_AnyState)
                modelModifier = editContext.GetModelModifier(viewModelModifier.PersistentState.ResourceUri, isReadOnly: true);
        
            if (viewModelModifier.PersistentState.ShouldRevealCursor)
            {
                ViewModel_RevealCursor(
                    editContext,
                    modelModifier,
                    viewModelModifier);
            }
            
            // This if expression exists below, to check if 'CalculateVirtualizationResult(...)' should be invoked.
            //
            // But, note that these cannot be combined at the bottom, we need to check if an edit
            // reduced the scrollWidth or scrollHeight of the editor's content.
            // 
            // This is done here, so that the 'ScrollWasModified' bool can be set, and downstream if statements will be entered,
            // which go on to scroll the editor.
            if (viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult)
            {
                ValidateMaximumScrollLeftAndScrollTop(editContext, modelModifier, viewModelModifier, textEditorDimensionsChanged: false);
            }

            if (!viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult &&
                viewModelModifier.ScrollWasModified)
            {
                // If not already going to reload virtualization result,
                // then check if the virtualization needs to be refreshed due to a
                // change in scroll position.
                //
                // This code only needs to run if the scrollbar was modified.
                
                if (viewModelModifier.Virtualization.Count > 0)
                {
                    if (viewModelModifier.PersistentState.ScrollTop < viewModelModifier.Virtualization.VirtualTop)
                    {
                        viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
                    }
                    else
                    {
                        var bigTop = viewModelModifier.PersistentState.ScrollTop + viewModelModifier.PersistentState.TextEditorDimensions.Height;
                        var virtualEnd = viewModelModifier.Virtualization.VirtualTop + viewModelModifier.Virtualization.VirtualHeight;
                            
                        if (bigTop > virtualEnd)
                            viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
                    }
                }
                
                // A check for horizontal virtualization still needs to be done.
                //
                // If we didn't already determine the necessity of calculating the virtualization
                // result when checking the vertical virtualization, then we check horizontal.
                if (!viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult)
                {
                    var scrollLeft = viewModelModifier.PersistentState.ScrollLeft;
                    if (scrollLeft < (viewModelModifier.Virtualization.VirtualLeft))
                    {
                        viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
                    }
                    else
                    {
                        var bigLeft = scrollLeft + viewModelModifier.PersistentState.TextEditorDimensions.Width;
                        if (bigLeft > viewModelModifier.Virtualization.VirtualLeft + viewModelModifier.Virtualization.VirtualWidth)
                            viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult = true;
                    }
                }
            }

            if (viewModelModifier.Virtualization.ShouldCalculateVirtualizationResult)
            {
                var componentData = viewModelModifier.PersistentState.ComponentData;
                if (componentData is not null)
                {
                    // TODO: This 'CalculateVirtualizationResultFactory' invocation is horrible for performance.
                    editContext.TextEditorService.ViewModel_CalculateVirtualizationResult(
                        editContext,
                        modelModifier,
                        viewModelModifier,
                        componentData);
                }
            }
            else if (viewModelModifier.PersistentState.Changed_Cursor_AnyState)
            {
                // If `CalculateVirtualizationResult` is invoked, then `CalculateCursorUi`
                // gets invoked as part of `CalculateVirtualizationResult`.
                //
                // This is done to permit the `CalculateCursorUi` to use the cache even if
                // the cursor isn't on screen.
                //
                // (otherwise `CalculateVirtualizationResult` would remove that line index from the cache,
                //  since it was offscreen).
            
                var componentData = viewModelModifier.PersistentState.ComponentData;
                if (componentData is not null)
                {
                    viewModelModifier.PersistentState.Changed_Cursor_AnyState = false;
                
                    viewModelModifier.Virtualization = new TextEditorVirtualizationResult(
                        modelModifier,
                        viewModelModifier,
                        componentData,
                        viewModelModifier.Virtualization);

                    viewModelModifier.Virtualization.External_GetCursorCss();
                    componentData.Virtualization = viewModelModifier.Virtualization;
                }
            }
            
            TextEditorState._viewModelMap[viewModelModifier.PersistentState.ViewModelKey] = viewModelModifier;
        }
        
        // __DiffModelCache.Clear();
        
        __ModelList.Clear();
        __ViewModelList.Clear();

        TextEditorStateChanged?.Invoke();
        
        // SetModelAndViewModelRange(editContext);
    }
    
    /// <summary>
    /// The argument 'bool textEditorDimensionsChanged' was added on (2024-09-20).
    /// 
    /// The issue is that this method was originally written for fixing the scrollLeft or scrollTop
    /// when the scrollWidth or scrollHeight changed to a smaller value.
    ///
    /// The if statements therefore check that the originalScrollWidth was higher,
    /// because some invokers of this method won't need to take time doing this calculation.
    ///
    /// For example, a pure insertion of text won't need to run this method. So the if statements
    /// allow for a quick exit.
    ///
    /// But, it was discovered that this same logic is needed when the text editor width changes.
    ///
    /// The text editor width changing results in a very specific event firing. So if we could
    /// just have a bool here to say, "I'm that specific event" then we can know that the width changed.
    /// 
    /// Because we cannot track the before and after of the width from this method since it already was changed.
    /// We need the specific event to instead tell us that it had changed.
    /// 
    /// So, the bool is a bit hacky.
    /// </summary>
    public void ValidateMaximumScrollLeftAndScrollTop(
        TextEditorEditContext editContext,
        TextEditorModel? modelModifier,
        TextEditorViewModel viewModelModifier,
        bool textEditorDimensionsChanged)
    {
        if (modelModifier is null)
            return;
        
        var originalScrollWidth = viewModelModifier.PersistentState.ScrollWidth;
        var originalScrollHeight = viewModelModifier.PersistentState.ScrollHeight;
        var tabWidth = editContext.TextEditorService.Options_GetOptions().TabWidth;
    
        var totalWidth = (int)Math.Ceiling(modelModifier.MostCharactersOnASingleLineTuple.lineLength *
            viewModelModifier.PersistentState.CharAndLineMeasurements.CharacterWidth);

        // Account for any tab characters on the 'MostCharactersOnASingleLineTuple'
        //
        // TODO: This code is not fully correct...
        //       ...if the longest line is 50 non-tab characters,
        //       and the second longest line is 49 tab characters,
        //       this code will erroneously take the '50' non-tab characters
        //       to be the longest line.
        {
            var lineIndex = modelModifier.MostCharactersOnASingleLineTuple.lineIndex;
            var longestLineInformation = modelModifier.GetLineInformation(lineIndex);

            var tabCountOnLongestLine = modelModifier.GetTabCountOnSameLineBeforeCursor(
                longestLineInformation.Index,
                longestLineInformation.LastValidColumnIndex);

            // 1 of the character width is already accounted for
            var extraWidthPerTabKey = tabWidth - 1;

            totalWidth += (int)Math.Ceiling(extraWidthPerTabKey *
                tabCountOnLongestLine *
                viewModelModifier.PersistentState.CharAndLineMeasurements.CharacterWidth);
        }

        var totalHeight = (modelModifier.LineEndList.Count) *
            viewModelModifier.PersistentState.CharAndLineMeasurements.LineHeight;

        // Add vertical margin so the user can scroll beyond the final line of content
        int marginScrollHeight;
        {
            var percentOfMarginScrollHeightByPageUnit = 0.4;

            marginScrollHeight = (int)Math.Ceiling(viewModelModifier.PersistentState.TextEditorDimensions.Height * percentOfMarginScrollHeightByPageUnit);
            totalHeight += marginScrollHeight;
        }

        viewModelModifier.PersistentState.ScrollWidth = totalWidth;
        viewModelModifier.PersistentState.ScrollHeight = totalHeight;
        viewModelModifier.PersistentState.MarginScrollHeight = marginScrollHeight;
        
        // var validateScrollWidth = totalWidth;
        // var validateScrollHeight = totalHeight;
        // var validateMarginScrollHeight = marginScrollHeight;
        
        if (originalScrollWidth > viewModelModifier.PersistentState.ScrollWidth ||
            textEditorDimensionsChanged)
        {
            viewModelModifier.SetScrollLeft(
                (int)viewModelModifier.PersistentState.ScrollLeft,
                viewModelModifier.PersistentState.TextEditorDimensions);
        }
        
        if (originalScrollHeight > viewModelModifier.PersistentState.ScrollHeight ||
            textEditorDimensionsChanged)
        {
            viewModelModifier.SetScrollTop(
                (int)viewModelModifier.PersistentState.ScrollTop,
                viewModelModifier.PersistentState.TextEditorDimensions);
            
            // The scrollLeft currently does not have any margin. Therefore subtracting the margin isn't needed.
            //
            // For scrollTop however, if one does not subtract the MarginScrollHeight in the case of
            // 'textEditorDimensionsChanged'
            //
            // Then a "void" will render at the top portion of the text editor, seemingly the size
            // of the MarginScrollHeight.
            if (textEditorDimensionsChanged &&
                viewModelModifier.PersistentState.ScrollTop != viewModelModifier.PersistentState.ScrollTop) // TODO: Why are these comparing eachother?
            {
                viewModelModifier.SetScrollTop(
                    (int)viewModelModifier.PersistentState.ScrollTop - (int)viewModelModifier.PersistentState.MarginScrollHeight,
                    viewModelModifier.PersistentState.TextEditorDimensions);
            }
        }
        
        var changeOccurred =
            viewModelModifier.PersistentState.ScrollLeft != viewModelModifier.PersistentState.ScrollLeft || // TODO: Why are these comparing eachother?
            viewModelModifier.PersistentState.ScrollTop != viewModelModifier.PersistentState.ScrollTop; // TODO: Why are these comparing eachother?
        
        if (changeOccurred)
        {
            viewModelModifier.ScrollWasModified = true;
        }
    }
    
    public async Task OpenInEditorAsync(
        TextEditorEditContext editContext,
        string absolutePath,
        bool shouldSetFocusToEditor,
        int? cursorPositionIndex,
        Category category,
        Key<TextEditorViewModel> preferredViewModelKey)
    {
        var resourceUri = new ResourceUri(absolutePath);
        var actualViewModelKey = await CommonLogic_OpenInEditorAsync(
            editContext,
            resourceUri,
            shouldSetFocusToEditor,
            category,
            preferredViewModelKey);
        
        var modelModifier = editContext.GetModelModifier(resourceUri);
        var viewModelModifier = editContext.GetViewModelModifier(actualViewModelKey);
        if (modelModifier is null || viewModelModifier is null)
            return;
            
        if (cursorPositionIndex is not null)
        {
            var lineAndColumnIndices = modelModifier.GetLineAndColumnIndicesFromPositionIndex(cursorPositionIndex.Value);
            viewModelModifier.LineIndex = lineAndColumnIndices.lineIndex;
            viewModelModifier.ColumnIndex = lineAndColumnIndices.columnIndex;
        }
        
        viewModelModifier.PersistentState.ShouldRevealCursor = true;
        FireAndForgetTask_OpenInTextEditor(actualViewModelKey, shouldSetFocusToEditor);
    }
    
    public async Task OpenInEditorAsync(
        TextEditorEditContext editContext,
        string absolutePath,
        bool shouldSetFocusToEditor,
        int? lineIndex,
        int? columnIndex,
        Category category,
        Key<TextEditorViewModel> preferredViewModelKey)
    {
        // Standardize Resource Uri
        if (TextEditorConfig.AbsolutePathStandardizeFunc is null)
            return;
            
        var standardizedFilePathString = await TextEditorConfig.AbsolutePathStandardizeFunc
            .Invoke(absolutePath, CommonService)
            .ConfigureAwait(false);
            
        var resourceUri = new ResourceUri(standardizedFilePathString);

        var actualViewModelKey = await CommonLogic_OpenInEditorAsync(
            editContext,
            resourceUri,
            shouldSetFocusToEditor,
            category,
            preferredViewModelKey);
            
        var modelModifier = editContext.GetModelModifier(resourceUri);
        var viewModelModifier = editContext.GetViewModelModifier(actualViewModelKey);

        if (modelModifier is null || viewModelModifier is null)
            return;
        
        if (lineIndex is not null)
            viewModelModifier.LineIndex = lineIndex.Value;
        if (columnIndex is not null)
            viewModelModifier.ColumnIndex = columnIndex.Value;
        
        if (viewModelModifier.LineIndex > modelModifier.LineCount - 1)
            viewModelModifier.LineIndex = modelModifier.LineCount - 1;
        
        var lineInformation = modelModifier.GetLineInformation(viewModelModifier.LineIndex);
        
        if (viewModelModifier.ColumnIndex > lineInformation.LastValidColumnIndex)
            viewModelModifier.SetColumnIndexAndPreferred(lineInformation.LastValidColumnIndex);
            
        viewModelModifier.PersistentState.ShouldRevealCursor = true;
        FireAndForgetTask_OpenInTextEditor(actualViewModelKey, shouldSetFocusToEditor);
    }
    
    private void FireAndForgetTask_OpenInTextEditor(Key<TextEditorViewModel> actualViewModelKey, bool shouldSetFocusToEditor)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(150).ConfigureAwait(false);
            WorkerArbitrary.PostUnique(editContext =>
            {
                var viewModelModifier = editContext.GetViewModelModifier(actualViewModelKey);
                viewModelModifier.PersistentState.ShouldRevealCursor = true;
                
                if (shouldSetFocusToEditor)
                    return viewModelModifier.FocusAsync();
                return ValueTask.CompletedTask;
            });
        });
    }
    
    /// <summary>
    /// Returns Key<TextEditorViewModel>.Empty if it failed to open in editor.
    /// Returns the ViewModel's key (non Key<TextEditorViewModel>.Empty value) if it successfully opened in editor.
    /// </summary>
    private async Task<Key<TextEditorViewModel>> CommonLogic_OpenInEditorAsync(
        TextEditorEditContext editContext,
        ResourceUri resourceUri,
        bool shouldSetFocusToEditor,
        Category category,
        Key<TextEditorViewModel> preferredViewModelKey)
    {
        // RegisterModelFunc
        if (TextEditorConfig.RegisterModelFunc is null)
            return Key<TextEditorViewModel>.Empty;
        await TextEditorConfig.RegisterModelFunc
            .Invoke(new RegisterModelArgs(editContext, resourceUri, CommonService, IdeBackgroundTaskApi))
            .ConfigureAwait(false);
    
        // TryRegisterViewModelFunc
        if (TextEditorConfig.TryRegisterViewModelFunc is null)
            return Key<TextEditorViewModel>.Empty;
        var actualViewModelKey = await TextEditorConfig.TryRegisterViewModelFunc
            .Invoke(new TryRegisterViewModelArgs(editContext, preferredViewModelKey, resourceUri, category, shouldSetFocusToEditor, CommonService, IdeBackgroundTaskApi))
            .ConfigureAwait(false);
    
        // TryShowViewModelFunc
        if (actualViewModelKey == Key<TextEditorViewModel>.Empty || TextEditorConfig.TryShowViewModelFunc is null)
            return Key<TextEditorViewModel>.Empty;
        await TextEditorConfig.TryShowViewModelFunc
            .Invoke(new TryShowViewModelArgs(actualViewModelKey, Key<TextEditorGroup>.Empty, shouldSetFocusToEditor, CommonService, IdeBackgroundTaskApi))
            .ConfigureAwait(false);
        
        return actualViewModelKey;
    }
    
    // Move TextEditorState.Reducer.cs here
    public void RegisterModel(TextEditorEditContext editContext, TextEditorModel model)
    {
        var inState = TextEditorState;
    
        var exists = inState._modelMap.TryGetValue(
            model.PersistentState.ResourceUri,
            out _);
    
        if (exists)
            return;
    
        inState._modelMap.Add(model.PersistentState.ResourceUri, model);
    
        TextEditorStateChanged?.Invoke();
    }

    /// <summary>
    /// WARNING/TODO: This method needs to remove from the TextEditorEditContext the removed model...
    /// ...because FinalizePost(...) writes back to the Dictionary but the key won't exist.
    ///
    /// The app doesn't have a case where this is a thing, since an edit context that solely is used to invoke DisposeModel(...)
    /// would throw a caught exception and then things just "move on".
    /// </summary>
    public void DisposeModel(TextEditorEditContext editContext, ResourceUri resourceUri)
    {
        var inState = TextEditorState;
    
        var exists = inState._modelMap.TryGetValue(
            resourceUri,
            out var model);
    
        if (!exists)
            return;
            
        foreach (var viewModelKey in model.PersistentState.ViewModelKeyList)
        {
            DisposeViewModel(editContext, viewModelKey);
        }
    
        inState._modelMap.Remove(resourceUri);
    
        TextEditorStateChanged?.Invoke();
    }
    
    public void SetModel(
        TextEditorEditContext editContext,
        TextEditorModel modelModifier)
    {
        var inState = TextEditorState;

        var exists = inState._modelMap.TryGetValue(
            modelModifier.PersistentState.ResourceUri, out var inModel);

        if (!exists)
            return;

        inState._modelMap[inModel.PersistentState.ResourceUri] = modelModifier;

        TextEditorStateChanged?.Invoke();
    }
    
    /// <summary>
    /// WARNING/TODO: This method needs to remove from the TextEditorEditContext the removed viewmodel...
    /// ...because FinalizePost(...) writes back to the Dictionary but the key won't exist.
    ///
    /// The app doesn't have a case where this is a thing, since an edit context that solely is used to invoke DisposeModel(...)
    /// would throw a caught exception and then things just "move on".
    /// </summary>
    public void RegisterViewModel(TextEditorEditContext editContext, TextEditorViewModel viewModel)
    {
        var inState = TextEditorState;
    
        var modelExisting = inState._modelMap.TryGetValue(
            viewModel.PersistentState.ResourceUri,
            out var model);
    
        if (!modelExisting)
            return;
    
        if (viewModel.PersistentState.ViewModelKey == Key<TextEditorViewModel>.Empty)
            throw new InvalidOperationException($"Provided {nameof(Key<TextEditorViewModel>)} cannot be {nameof(Key<TextEditorViewModel>)}.{Key<TextEditorViewModel>.Empty}");
    
        var viewModelExisting = inState.ViewModelGetOrDefault(viewModel.PersistentState.ViewModelKey);
        if (viewModelExisting is not null)
            return;
    
        model.PersistentState.ViewModelKeyList.Add(viewModel.PersistentState.ViewModelKey);
    
        inState._viewModelMap.Add(viewModel.PersistentState.ViewModelKey, viewModel);
    
        TextEditorStateChanged?.Invoke();
    }
    
    public void DisposeViewModel(TextEditorEditContext editContext, Key<TextEditorViewModel> viewModelKey)
    {
        var inState = TextEditorState;
        
        var viewModel = inState.ViewModelGetOrDefault(viewModelKey);
        if (viewModel is null)
            return;
        
        inState._viewModelMap.Remove(viewModel.PersistentState.ViewModelKey);
        viewModel.Dispose();
    
        var model = inState.ModelGetOrDefault(viewModel.PersistentState.ResourceUri);
        if (model is not null)
            model.PersistentState.ViewModelKeyList.Remove(viewModel.PersistentState.ViewModelKey);
        
        TextEditorStateChanged?.Invoke();
    }
    
    public void SetModelAndViewModelRange(TextEditorEditContext editContext)
    {
        // TextEditorState isn't currently being re-instantiated after the state is modified, so I'm going to comment out this local reference.
        // 
        // var inState = TextEditorState;

        if (__ModelList.Count > 0)
        {
            foreach (var model in __ModelList)
            {
                if (TextEditorState._modelMap.ContainsKey(model.PersistentState.ResourceUri))
                    TextEditorState._modelMap[model.PersistentState.ResourceUri] = model;
            }
            
            __ModelList.Clear();
        }
        
        if (__ViewModelList.Count > 0)
        {
            foreach (var viewModel in __ViewModelList)
            {
                if (TextEditorState._viewModelMap.ContainsKey(viewModel.PersistentState.ViewModelKey))
                    TextEditorState._viewModelMap[viewModel.PersistentState.ViewModelKey] = viewModel;
            }
            
            __ViewModelList.Clear();
        }

        TextEditorStateChanged?.Invoke();
    }
    
    public void Enqueue_TextEditorInitializationBackgroundTaskGroupWorkKind()
    {
        CommonService.Continuous_Enqueue(new BackgroundTask(
            Key<IBackgroundTaskGroup>.Empty,
            Do_WalkTextEditorInitializerOnInit));
    }

    public async ValueTask Do_WalkTextEditorInitializerOnInit()
    {
        CommonService.Theme_RegisterAction(TextEditorConfig.CustomThemeOne);
        CommonService.Theme_RegisterAction(TextEditorConfig.CustomThemeTwo);

        var initialThemeRecord = CommonService.GetThemeState().ThemeList.FirstOrDefault(
            x => x.Key == TextEditorConfig.InitialThemeKey);

        if (initialThemeRecord != default)
            Options_SetTheme(initialThemeRecord, updateStorage: false);

        await Options_SetFromLocalStorageAsync().ConfigureAwait(false);

        CommonService.RegisterKeymapLayer(TextEditorFacts.KeymapDefault_HasSelectionLayer);
    }
}
