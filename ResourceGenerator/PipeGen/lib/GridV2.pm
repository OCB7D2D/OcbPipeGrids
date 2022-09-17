use strict;
use warnings;
package GridV2;
use POSIX qw(fmod);
use Math::Vector::Real;

sub V2GridParam
{
	return $_[0] < 0
		? $_[0] - fmod($_[0] - $_[1]/2, $_[1]) - $_[1]/2
		: $_[0] - fmod($_[0] + $_[1]/2, $_[1]) + $_[1]/2;
}

sub V2Grid
{
	return V(
		V2GridParam($_[0]->[0], $_[1]),
		V2GridParam($_[0]->[1], $_[1]));
}

sub V2Key
{
	return sprintf("%0.6f %0.6f",
		$_[0]->[0], $_[0]->[1]);
}

sub new {
	bless {
		delta => $_[1],
		count => 0,
		list => [],
		grid => {},
		lookup => {},
		vertices => {},
	}, $_[0];
}

sub Add
{
	my $self = shift;
	foreach my $v (@_)
	{
		my $vg = V2Grid($v, $self->{delta});
		my $key = V2Key($vg);
		if (exists $self->{grid}->{$key})
		{
			$v->[2] =
			$self->{lookup}->{$v} =
				$self->{grid}->{$key};
			push @{$self->{vertices}->{$key}}, $v;
		}
		else
		{
			$v->[2] =
			$self->{lookup}->{$v} =
			$self->{grid}->{$key} =
				$self->{count} ++;
			push @{$self->{list}}, $vg;
			$self->{vertices}->{$key} = [$v];
		}
	}
}

sub GetVertices
{
	my ($self, $v) = @_;
	my $bg = V2Key(V2Grid($v, $self->{delta}));
	return @{$_[0]->{vertices}->{$bg}};
}

1;