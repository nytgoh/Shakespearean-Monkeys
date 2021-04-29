# Shakespearean-Monkeys
A third year project to create a distributed application that illustrates the Shakespearean Monkeys genetic algorithm.

## What it does
This project consists of three locally hosted servers that request and respond to requests between one another. The goal is for the Monkeys to figure out what word the Client sent to the Fitness server based on fitness values that the Monkeys can request from the Fitness server.
- Fitness: The server with the target word. It calculates a fitness value depending on how close the word given is to the target.
- Monkey: The server trying to figure out the target word. It follows the parameters given to it by the client.
- Client: The server that sends Fitness the target word and Monkey the parameters it should have when trying to find the word.

![image](https://user-images.githubusercontent.com/54729791/116620258-b06bad00-a995-11eb-9683-f4a80c0e92f1.png)

## How to run it
1. Open 3 powershell windows, one per server.
2. In each window, choose the directory of each server folder. I.e.  `cd "D:\COMPSCI335\ngoh852-monkeys"` for the Monkeys server window.
3. In their respective windows, run the csproject using `dotnet run -p Monkeys.csproj`, `dotnet run -p Fitness.csproj` and `dotnet run -p Client.csproj`. Note that the first two must be run before the Client.

- The default word that the Client will test the algorithm on is "abcd". This can be changed in `Client.cs` at the commented point (line 67).
- The Client server may be replaced with manual POST requests as well. Testing was done with  [Postman](https://learning.postman.com/).
  - Note that the Target POST must be done before the Try POST. See below.
  
  ![image](https://user-images.githubusercontent.com/54729791/116623274-1ce8ab00-a99a-11eb-9e30-a708431bc60a.png)

