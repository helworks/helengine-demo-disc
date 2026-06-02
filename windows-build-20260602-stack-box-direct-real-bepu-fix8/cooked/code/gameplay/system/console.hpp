#ifndef CONSOLE_HPP
#define CONSOLE_HPP

#include <string>

class Console {
public:
	static bool Write(char* fileName);
	static bool Write(const std::string& text);
	static bool WriteLine(char* fileName);
	static bool WriteLine(const std::string& text);
	static bool WriteLine();
	static std::string ReadLine();
};

#endif // CONSOLE_HPP
