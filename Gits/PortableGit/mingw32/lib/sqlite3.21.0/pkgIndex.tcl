#
# Tcl package index file
#
# Note sqlite*3* init specifically
#
package ifneeded sqlite3 3.21.0 \
    [list load [file join $dir sqlite3210.dll] Sqlite3]
