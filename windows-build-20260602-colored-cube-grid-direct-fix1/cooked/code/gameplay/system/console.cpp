#include "console.hpp"
#include "io/file.hpp"
#include <iostream>

using namespace std;

bool Console::Write(char* fileName)
{
	cout << fileName;
    return true;
}

bool Console::Write(const std::string& text)
{
	cout << text;
    return true;
}

bool Console::WriteLine(char* fileName) 
{
	cout << fileName << std::endl;
    return true;
}

bool Console::WriteLine(const std::string& text)
{
	cout << text << std::endl;
    return true;
}

bool Console::WriteLine()
{
	cout << std::endl;
    return true;
}

std::string Console::ReadLine()
{
    std::string line;
    std::getline(cin, line);
    return line;
}
