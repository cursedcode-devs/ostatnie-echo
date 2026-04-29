# ZASADY REPO

**Scenę z grą może modyfikować _tylko jedna osoba_ na raz aby uniknąć merge conflictów.**
Aby udało się to osiągnąć, musimy zarządzać tym kto w danej chwili pracuje na scenie z grą.
Jako main programistę mianuję na tą rolę @Wojtek (lub mnie jeśli Wojtek będzie niedostępny).

## Jak pracować nad grą bez modyfikowania sceny

Być może zastanawiacie się teraz jak macie pracować nad grą jeśli nie możecie modyfikować scen. Odpowiedzią są kopie głównej sceny i prefaby.
Żeby to lepiej zrozumieć przedstawię przykład.

## Przykładowe zadanie

### Dodać terminale do gry.
Osoba wykonująca to zadanie nie może bezpośrednio zmodyfikować sceny z grą, na której znajduje się mapa. Musi jednak poustawiać terminale w odpowiednich miejscach na tejże mapie.
**Tworzy więc kopię sceny _SampleScene_**, która jest w folderze **_Scenes_** i **umieszcza ją w swoim folderze developerskim np. _"Assets/dev/cedud"_** (zmieniając nazwę, np. dopisując nr zadania z githuba: game-123).
Następnie rozstawia terminale na tej kopii sceny. Po ich rozstawieniu **tworzy pusty GameObject i przypisuje wszystkie terminale jako jego dzieci**,
po czym przeciąga go do swojego folderu, tworząc w ten sposób prefab. ***Nie usuwa natomiast sceny*** na której pracowała, ponieważ będzie ona przydatna do review. Następnie zmiany są przeglądane i finalnie zatwierdzane.

Rezultatem takiego zadania jest GameObject zawierajacy wszystkie zamontowane monitory, które są przerzucane do sceny głównej i tam rozpakowywane.
