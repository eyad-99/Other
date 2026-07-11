using StackExchange.Redis;

var muxer = ConnectionMultiplexer.Connect("localhost:6379"); 



var db = muxer.GetDatabase();


//string 
var instructorNameKey = new RedisKey("instructors:1:name");
db.StringSet(instructorNameKey, "Steve");
var instructor1Name = db.StringGet(instructorNameKey);
Console.WriteLine($"Instructor 1's name is: {instructor1Name}");
var tempKey = "temperature";
db.StringSet(tempKey, 42);
var tempAsLong = db.StringIncrement(tempKey, 5);
Console.WriteLine($"New temperature: {tempAsLong}");


//list

var fruitKey = "fruits";
var vegetableKey = "vegetables";
db.KeyDelete(new RedisKey[] { fruitKey, vegetableKey });
db.ListLeftPush(fruitKey, new RedisValue[] { "Banana", "Mango", "Apple", "Pepper", "Kiwi", "Grape" });
Console.WriteLine($"The first fruit in the list is: {db.ListGetByIndex(fruitKey, 0)}");
Console.WriteLine($"The last fruit in the list is:  {db.ListGetByIndex(fruitKey, -1)}");
db.ListRightPush(vegetableKey, new RedisValue[] { "Potato", "Carrot", "Asparagus", "Beet", "Garlic", "Tomato" });
Console.WriteLine($"The first vegetable in the list is: {db.ListGetByIndex(vegetableKey, 0)}");
Console.WriteLine($"Position of Mango: {db.ListPosition(fruitKey, "Mango")}");
Console.WriteLine($"There are {db.ListLength(fruitKey)} fruits in our Fruit List");




//set

var allUsersSet = "users";
var activeUsersSet = "users:state:active";
var inactiveUsersSet = "users:state:inactive";
var offlineUsersSet = "users:state:offline";
db.KeyDelete(new RedisKey[] { allUsersSet, activeUsersSet, inactiveUsersSet, offlineUsersSet });
db.SetAdd(activeUsersSet, new RedisValue[] { "User:1", "User:2" });
db.SetAdd(inactiveUsersSet, new RedisValue[] { "User:3", "User:4" });
db.SetAdd(offlineUsersSet, new RedisValue[] { "User:5", "User:6", "User:7" });
var user6Offline = db.SetContains(offlineUsersSet, "User:6");
Console.WriteLine($"User:6 offline: {user6Offline}");
Console.WriteLine($"All Users In one shot: {string.Join(", ", db.SetMembers(allUsersSet))}");
Console.WriteLine($"All Users with scan  : {string.Join(", ", db.SetScan(allUsersSet))}");
Console.WriteLine("Moving User:1 from active to offline");
var moved = db.SetMove(activeUsersSet, offlineUsersSet, "User:1");
Console.WriteLine($"Move Successful: {moved}");


//sorted set

var userAgeSet = "users:age";
var userLastAccessSet = "users:lastAccess";
var userHighScoreSet = "users:highScores";
var namesSet = "names";
var mostRecentlyActive = "users:mostRecentlyActive";
db.KeyDelete(new RedisKey[] { userAgeSet, userLastAccessSet, userHighScoreSet, namesSet, mostRecentlyActive });
db.SortedSetAdd(userAgeSet,
    new SortedSetEntry[]

    {

            new("User:1", 20),

            new("User:2", 23),

            new("User:3", 18),

            new("User:4", 35),

            new("User:5", 55),

            new("User:6", 62)

    });
db.SortedSetAdd(userLastAccessSet,

    new SortedSetEntry[]

    {

            new("User:1", 1648483867),

            new("User:2", 1658074397),

            new("User:3", 1659132660),

            new("User:4", 1652082765),

            new("User:5", 1658087415),

            new("User:6", 1656530099)

    });
db.SortedSetAdd(userHighScoreSet,

    new SortedSetEntry[]

    {

            new("User:1", 10),

            new("User:2", 55),

            new("User:3", 36),

            new("User:4", 25),

            new("User:5", 21),

            new("User:6", 44)

    });
db.SortedSetAdd(namesSet,

    new SortedSetEntry[]

    {

            new("John", 0),

            new("Fred", 0),

            new("Bob", 0),

            new("Susan", 0),

            new("Alice", 0),

            new("Tom", 0)

    });




//redis hash

//pubsub
var subscriber = muxer.GetSubscriber();
var cancellationTokenSource = new CancellationTokenSource();
var token = cancellationTokenSource.Token;
 subscriber.Subscribe("news", (channel, message) =>
{
    Console.WriteLine($"Received: {message}");
});

subscriber.Publish("news", "Hello subscribers!");
Thread.Sleep(1000);
cancellationTokenSource.Cancel();



