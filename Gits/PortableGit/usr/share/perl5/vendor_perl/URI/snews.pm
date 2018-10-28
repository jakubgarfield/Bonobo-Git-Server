package URI::snews;  # draft-gilman-news-url-01

use strict;
use warnings;

our $VERSION = '1.74';

use parent 'URI::news';

sub default_port { 563 }

sub secure { 1 }

1;
