Mała aplikacja która pozwala wyszukać wpisaną frazę wśród wszystkich wyrazów języka polskiego, w czasie krótszym niż 0.1ms. Plik words.txt zawiera 4603639 różnych słów o łącznej liczbie liter 60839419.

Wymaga pobrania ".NET Desktop Runtime 5.0.17" https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-5.0.17-windows-x64-installer

Przy pierwszym uruchomieniu tworzy przez kilka minut plik "wordsIndex.txt", który waży zwykle około 8 razy więcej niż plik "words.txt". 

![Alt text](/img/files.PNG?raw=true "Pliki aplikacji")

Po odczytaniu wszystkich plików pokaże się napis "Napisz coś i wciśnij enter ...", należy coś wtedy napisać i wcisnąć enter.

![Alt text](/img/fmindex.PNG?raw=true "Aplikacja konsolowa")

Po uruchomieniu aplikacji powinno od razu wyświetlić się słowo "START", jeżeli tak się nie stało trzeba kliknąć enter.

Słowa pochądzą z źródła https://sjp.pl/sl/odmiany/

Aplikacja do wyszukiwania wykorzystuje algorytm FM-index, zmodyfikowany do obsługi wielu łańcuchów znaków. Złożoność obliczeniowa wyszukiwania wynosi O(m), złożoność pamięciowa O(n), złożoność obliczeniowa budowania struktury O(n*log(n)). Gdzie m to długość wyszukiwanego wzorca, n liczba wszystkich liter w wszystkich tekstach. Plik wordsIndex.txt przechowuje łańcuch liter w formacie transformaty Burrowsa-Wheelera, do każdej litery przypisane są metadane dotyczące tego z jakiego słowa pochodzi litera (4 bajty), oraz z jakiego miejsca w słowie (2 bajty). Sama litera zajmuje 1 lub 2 bajty stąd rozmiar pliku struktury w najgorszym przypadku jest 4 + 2 + 2 razy większy niż oryginalny tekst. Słowo może mieć długość maksymalnie 65535 liter. Program do każdego słowa dopisuje na końcu znak $, jako koniec łańcucha znaków. Zamiast słów można teoretycznie użyć wielu różnych opisów lub artykułów, pod warunkiem że zmieszczą się w 65535 znakach, trzeba do tego zmodyfikować metodę „GetWords()” żeby inaczej dzieliła teksty na zmienne typu string.
