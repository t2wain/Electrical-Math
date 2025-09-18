# Excel API Abstraction Code Design

There are many libraries available to work with Excel file. However, each library has its own specific API that are quite different from each other. To avoid using vendor-specific API throughout the code-base, I have created a layer of abstraction for some Excel entities that are needed specifically in the code-base. 